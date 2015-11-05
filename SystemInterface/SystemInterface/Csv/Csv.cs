﻿using System;
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
		public readonly ReadOnlyCollection<string> Columns;

		private string _fileName;
		private string _fileNameTmp;
		private char _delimeter;

		public Csv(char delimeter, string filename, string fileNameTmp, params string[] columns)
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

			bool isFirst = true;
			foreach (string column in Columns)
			{
				if (isFirst == false)
				{
					streamWriter.Write(_delimeter);
				}

				streamWriter.Write(column);

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

		private ReadOnlyCollection<string> ReadColumnsFromFile()
		{
			StreamReader streamReader = GetReader();
			string firstLine = streamReader.ReadLine();

			if (firstLine == null || firstLine.Length == 0)
			{
				streamReader.Close();
				throw new Exception($"no columns found in file {_fileName}");
			}

			string[] parts = firstLine.Split(_delimeter);

			return parts.ToList().AsReadOnly();
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
				string columnsName = Columns[columnIndex];

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
			int keyIndex = keyNameToKeyIndex(keyName);

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

		public List<Dictionary<string, string>> ReadFields(string keyName, string keyValue)
		{
			Func<string, string, bool> compareFunc = (stringInCsv, stringInputValue) =>
			{
				return stringInCsv == stringInputValue;
			};

			Func<string, string> convertFunc = (value) => value;

			return ReadCompared(keyName, keyValue, compareFunc, convertFunc);
		}

		public List<Dictionary<string, string>> ReadLatest(string keyName, DateTime minimumDateToReturn)
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

			return ReadCompared(keyName, minimumDateToReturn, compareFunc, convertFunc);
		}

		public List<Dictionary<string, string>> ReadCompared<CompareType>(string keyName, CompareType compareObject, Func<CompareType, CompareType, bool> compareFunc, Func<string, CompareType> convertFunc)
		{
			int keyIndex = keyNameToKeyIndex(keyName);

			if (keyIndex > Columns.Count)
			{
				throw new ArgumentException($"keyIndex {keyIndex} out of range, there are only {Columns.Count} columns");
			}

			StreamReader streamReader = GetReader();

			List<Dictionary<string, string>> collectedValues = new List<Dictionary<string, string>>();

			while (streamReader.EndOfStream == false)
			{
				string line = streamReader.ReadLine();
				string[] parts = line.Split(_delimeter);

				CompareType objectInCsv = convertFunc(parts[keyIndex]);

				if (compareFunc(objectInCsv, compareObject))
				{
					Dictionary<string, string> rowValues = new Dictionary<string, string>();
					for (int columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
					{
						string key = Columns[columnIndex];
						string value = parts[columnIndex];

						rowValues.Add(key, value);
					}
					collectedValues.Add(rowValues);
				}
			}

			streamReader.Close();

			return collectedValues;
		}

		public void Delete(string keyName, string keyValue)
		{
			int keyIndex = keyNameToKeyIndex(keyName);

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

		private int keyNameToKeyIndex(string keyName)
		{
			if (Columns.Contains(keyName))
			{
				return Columns.IndexOf(keyName);
			}

			throw new ArgumentException($"keyName {keyName} not found");
		}
	}
}