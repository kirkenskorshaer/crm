using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SystemInterface.Csv
{
	public class Csv
	{
		public readonly ReadOnlyCollection<ColumnDefinition> Columns;

		private string _fileName;
		private string _fileNameTmp;
		private char _delimeter;

		public Csv(char delimeter, string filename, string fileNameTmp, params ColumnDefinition[] columns)
		{
			_fileName = filename;
			_fileNameTmp = fileNameTmp;

			_delimeter = delimeter;

			if (columns.Any())
			{
				Columns = columns.ToList().AsReadOnly();
				VerifyFileFormat();
				CreateHeadersIfNeeded();
			}
			else
			{
				Columns = ReadColumnsFromFile();
			}
		}

		private void CreateHeadersIfNeeded()
		{
			StreamReader streamReader = GetReader();
			StreamWriter streamWriter = GetWriter();

			string firstLine = streamReader.ReadLine();
			string firstLineShouldBe = Columns.Select(definition => definition.Name).Aggregate((current, next) => current + _delimeter + next);
			if (firstLine == firstLineShouldBe)
			{
				return;
			}

			bool isFirst = true;
			foreach (ColumnDefinition column in Columns)
			{
				if (isFirst == false)
				{
					streamWriter.Write(_delimeter);
				}

				streamWriter.Write(column.Name);

				isFirst = false;
			}

			streamReader.Close();

			streamWriter.Flush();
			streamWriter.Close();
		}

		private StreamWriter GetWriter()
		{
			FileStream fileStreamWrite = File.Open(_fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
			StreamWriter streamWriter = new StreamWriter(fileStreamWrite, Encoding.UTF8);
			return streamWriter;
		}

		private StreamWriter GetTmpWriter()
		{
			FileStream fileStreamWrite = File.Open(_fileNameTmp, FileMode.Create, FileAccess.Write);
			StreamWriter streamWriter = new StreamWriter(fileStreamWrite, Encoding.UTF8);
			return streamWriter;
		}

		private StreamReader GetReader()
		{
			FileStream fileStreamRead = File.Open(_fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
			StreamReader streamReader = new StreamReader(fileStreamRead, Encoding.UTF8);
			return streamReader;
		}

		private ReadOnlyCollection<ColumnDefinition> ReadColumnsFromFile()
		{
			StreamReader streamReader = GetReader();
			string firstLine = streamReader.ReadLine();

			if (firstLine == null || firstLine.Length == 0)
			{
				streamReader.Close();
				throw new Exception($"no columns found in file {_fileName}");
			}

			string[] parts = firstLine.Split(_delimeter);

			return parts.Select(part => new ColumnDefinition(ColumnDefinition.DataTypeEnum.stringType, part)).ToList().AsReadOnly();
		}

		private void VerifyFileFormat()
		{
			StreamReader streamReader = GetReader();
			string firstLine = streamReader.ReadLine();

			if (firstLine == null || firstLine.Length == 0)
			{
				streamReader.Close();
				return;
			}

			string[] parts = firstLine.Split(_delimeter);
			if (parts.Count() != Columns.Count)
			{
				streamReader.Close();
				throw new IOException($"Wrong column count in file {_fileName}, was {parts.Count()} expected {Columns.Count}");
			}

			for (int columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
			{
				string partsName = parts[columnIndex];
				string columnsName = Columns[columnIndex].Name;

				if (partsName != columnsName)
				{
					streamReader.Close();
					throw new IOException($"Wrong column name in file {_fileName}, was {partsName} expected {columnsName}");
				}
			}

			streamReader.Close();
		}

		public void WriteLine(params string[] values)
		{
			if (values.Count() != Columns.Count)
			{
				throw new ArgumentException($"wrong argument count, current Csv file expects {Columns.Count} parameters");
			}

			StreamWriter streamWriter = GetWriter();
			streamWriter.BaseStream.Position = streamWriter.BaseStream.Length;
			streamWriter.WriteLine();

			for (int columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
			{
				string value = values[columnIndex];

				if (columnIndex != 0)
				{
					streamWriter.Write(_delimeter);
				}

				streamWriter.Write(value);
			}

			streamWriter.Flush();
			streamWriter.Close();
		}

		public void Update(string keyName, string keyValue, params string[] values)
		{
			int keyIndex = columnNameToColumnIndex(keyName);

			if (values.Count() != Columns.Count)
			{
				throw new ArgumentException($"wrong argument count, current Csv file expects {Columns.Count} parameters");
			}

			if (keyIndex > Columns.Count)
			{
				throw new ArgumentException($"keyIndex {keyIndex} out of range, there are only {Columns.Count} columns");
			}

			StreamWriter streamWriterTmp = GetTmpWriter();

			StreamReader streamReader = GetReader();

			string line = streamReader.ReadLine();
			streamWriterTmp.WriteLine(line);

			bool isFirst = true;
			while (streamReader.EndOfStream == false)
			{
				line = streamReader.ReadLine();

				if (isFirst == false)
				{
					streamWriterTmp.WriteLine();
				}

				string[] parts = line.Split(_delimeter);

				if (parts[keyIndex] == keyValue)
				{
					line = values.Aggregate((colletor, part) => colletor + _delimeter + part);
				}

				streamWriterTmp.Write(line);

				isFirst = false;
			}

			streamReader.Close();

			streamWriterTmp.Flush();
			streamWriterTmp.Close();

			File.Delete(_fileName);
			File.Move(_fileNameTmp, _fileName);
		}

		public List<Dictionary<string, object>> ReadFields(string keyName, string keyValue)
		{
			Func<string, string, bool> compareFunc = (stringInCsv, stringInputValue) =>
			{
				return stringInCsv == stringInputValue;
			};

			Func<string, string> convertFunc = (value) => value;

			return ReadCompared(keyName, keyValue, compareFunc, convertFunc);
		}

		public List<Dictionary<string, object>> ReadLatest(string keyName, string dateName, DateTime minimumDateToReturn)
		{
			Func<DateTime, DateTime, bool> compareFunc = (DateTime dateInCsv, DateTime minimumDate) =>
			{
				return dateInCsv >= minimumDate;
			};

			Func<string, DateTime> convertFunc = (input) =>
			{
				DateTime convertedDateTime;
				DateTime.TryParseExact(input, "yyyyMMdd HH:mm:ss", null, DateTimeStyles.None, out convertedDateTime);

				return convertedDateTime;
			};

			return ReadCompared(dateName, minimumDateToReturn, compareFunc, convertFunc);
		}

		public List<Dictionary<string, object>> ReadCompared<CompareType>(string dateName, CompareType compareObject, Func<CompareType, CompareType, bool> compareFunc, Func<string, CompareType> convertFunc)
		{
			int dateIndex = columnNameToColumnIndex(dateName);

			if (dateIndex > Columns.Count)
			{
				throw new ArgumentException($"dateIndex {dateIndex} out of range, there are only {Columns.Count} columns");
			}

			StreamReader streamReader = GetReader();

			List<Dictionary<string, object>> collectedValues = new List<Dictionary<string, object>>();

			while (streamReader.EndOfStream == false)
			{
				string line = streamReader.ReadLine();
				string[] parts = line.Split(_delimeter);

				CompareType objectInCsv = convertFunc(parts[dateIndex]);

				if (compareFunc(objectInCsv, compareObject))
				{
					Dictionary<string, object> rowValues = new Dictionary<string, object>();
					for (int columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
					{
						string key = Columns[columnIndex].Name;
						object value = GetValue(parts, columnIndex);

						rowValues.Add(key, value);
					}
					collectedValues.Add(rowValues);
				}
			}

			streamReader.Close();

			return collectedValues;
		}

		public List<Dictionary<string, object>> ReadAll()
		{
			StreamReader streamReader = GetReader();

			List<Dictionary<string, object>> collectedValues = new List<Dictionary<string, object>>();

			bool IsHeaderRow = true;

			while (streamReader.EndOfStream == false)
			{
				string line = streamReader.ReadLine();

				if (IsHeaderRow)
				{
					IsHeaderRow = false;
					continue;
				}

				string[] parts = line.Split(_delimeter);

				Dictionary<string, object> rowValues = new Dictionary<string, object>();
				for (int columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
				{
					string key = Columns[columnIndex].Name;
					object value = GetValue(parts, columnIndex);

					rowValues.Add(key, value);
				}
				collectedValues.Add(rowValues);
			}

			streamReader.Close();

			return collectedValues;
		}

		private object GetValue(string[] parts, int columnIndex)
		{
			ColumnDefinition definition = Columns[columnIndex];

			return definition.GetValue(parts, columnIndex);
		}

		public void Delete(string keyName, string keyValue)
		{
			int keyIndex = columnNameToColumnIndex(keyName);

			if (keyIndex > Columns.Count)
			{
				throw new ArgumentException($"keyIndex {keyIndex} out of range, there are only {Columns.Count} columns");
			}

			StreamWriter streamWriterTmp = GetTmpWriter();

			StreamReader streamReader = GetReader();

			string line = streamReader.ReadLine();
			streamWriterTmp.WriteLine(line);

			bool isFirst = true;
			while (streamReader.EndOfStream == false)
			{
				line = streamReader.ReadLine();

				string[] parts = line.Split(_delimeter);

				if (parts[keyIndex] != keyValue)
				{
					if (isFirst == false)
					{
						streamWriterTmp.WriteLine();
					}

					streamWriterTmp.Write(line);

					isFirst = false;
				}
			}

			streamReader.Close();

			streamWriterTmp.Flush();
			streamWriterTmp.Close();

			File.Delete(_fileName);
			File.Move(_fileNameTmp, _fileName);
		}

		private int columnNameToColumnIndex(string columnName)
		{
			if (Columns.Select(definition => definition.Name).Contains(columnName))
			{
				ColumnDefinition columnDefinition = Columns.First(definition => definition.Name == columnName);
				return Columns.IndexOf(columnDefinition);
			}

			throw new ArgumentException($"column name {columnName} not found");
		}
	}
}
