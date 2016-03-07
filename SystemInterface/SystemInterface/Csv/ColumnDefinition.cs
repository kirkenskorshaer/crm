using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemInterface.Csv
{
	public class ColumnDefinition
	{
		public DataTypeEnum DataType { get; private set; }
		public string Name { get; private set; }

		public string DefinedName
		{
			get
			{
				switch (DataType)
				{
					case DataTypeEnum.boolType:
						return "bool:" + Name;
					case DataTypeEnum.intType:
						return "int:" + Name;
					default:
						return Name;
				}
			}
		}

		private static string[] positive = new string[] { "1", "true", "ja", "ok", "x" };

		public enum DataTypeEnum
		{
			stringType = 1,
			boolType = 2,
			intType = 3,
		}

		public ColumnDefinition(DataTypeEnum dataType, string name)
		{
			DataType = dataType;
			Name = name;
		}

		public static ColumnDefinition[] Read(string[] fields)
		{
			return fields.Select(field =>
			{
				if (field.Contains(":"))
				{
					string[] split = field.Split(':');
					return new ColumnDefinition(GetTypeFromString(split[0]), split[1]);
				}
				else
				{
					return new ColumnDefinition(DataTypeEnum.stringType, field);
				}
			}).ToArray();
		}

		private static DataTypeEnum GetTypeFromString(string stringType)
		{
			switch (stringType)
			{
				case "bool":
					return DataTypeEnum.boolType;
				case "int":
					return DataTypeEnum.intType;
				default:
					return DataTypeEnum.stringType;
			}
		}

		public static string[] ToDefinitionString(ColumnDefinition[] fields)
		{
			return fields.Select(field => field.DefinedName).ToArray();
		}

		public object GetValue(string[] parts, int columnIndex)
		{
			switch (DataType)
			{
				case DataTypeEnum.stringType:
					return parts[columnIndex];
				case DataTypeEnum.boolType:
					return positive.Contains(parts[columnIndex].ToLower());
				case DataTypeEnum.intType:
					return int.Parse(parts[columnIndex]);
				default:
					break;
			}
			return parts[columnIndex];
		}
	}
}
