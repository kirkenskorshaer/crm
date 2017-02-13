using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Utilities.Converter
{
	public static class StringConverter
	{
		public static string SerializeToString(object inputObject)
		{
			MemoryStream outputStream = new MemoryStream();
			string serialized = string.Empty;

			try
			{
				using (outputStream)
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(outputStream, inputObject);

					serialized = Convert.ToBase64String(outputStream.ToArray());
				}
			}
			catch (SerializationException)
			{
				throw;
			}
			finally
			{
				outputStream.Close();
			}

			return serialized;
		}

		public static object DeserializeFromString(string inputString)
		{
			byte[] inputBytes = Convert.FromBase64String(inputString);
			MemoryStream valueStream = new MemoryStream(inputBytes);

			object deserializedObject;

			try
			{
				using (valueStream)
				{
					BinaryFormatter formatter = new BinaryFormatter();
					deserializedObject = formatter.Deserialize(valueStream);
				}
			}
			catch (SerializationException)
			{
				throw;
			}
			finally
			{
				valueStream.Close();
			}

			return deserializedObject;
		}
	}
}
