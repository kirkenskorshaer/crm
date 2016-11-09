using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Utilities
{
	public class TextMerger
	{
		private string _originalText;
		private MatchCollection _matches;
		private CultureInfo _cultureInfo = new CultureInfo("da-DK");

		public TextMerger(string originalText)
		{
			_originalText = originalText;
			_matches = Regex.Matches(_originalText, "\\{![^\\}]*\\}");
		}

		public bool ContainsFields()
		{
			if (_matches == null)
			{
				return false;
			}

			return _matches.Count > 0;
		}

		public string GetMerged(IDictionary<string, object> fieldDictionary, string dateFormat)
		{
			string mergedText = _originalText;

			foreach (Match match in _matches)
			{
				string matchedString = match.Value;
				matchedString = matchedString.Substring(2, matchedString.Length - 3);
				string[] options = matchedString.Split(';');

				for (int optionIndex = 0; optionIndex < options.Length - 1; optionIndex++)
				{
					string option = options[optionIndex];
					if (fieldDictionary.ContainsKey(option))
					{
						object value = fieldDictionary[option];
						string stringValue;

						if (value is DateTime)
						{
							stringValue = ((DateTime)value).ToString(dateFormat, _cultureInfo);
						}
						else
						{
							stringValue = value.ToString();
						}

						mergedText = mergedText.Replace(match.Value, stringValue);
					}
				}

				mergedText = mergedText.Replace(match.Value, options.Last());
			}

			return mergedText;
		}
	}
}
