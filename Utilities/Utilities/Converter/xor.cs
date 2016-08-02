using System;
using System.Collections.Generic;
using System.Linq;
namespace Utilities.Converter
{
	public static class Xor
	{
		public static Guid XorGuid(Guid inputA, Guid inputB)
		{
			byte[] inputABytes = inputA.ToByteArray();
			byte[] inputBBytes = inputB.ToByteArray();

			byte[] resultBytes = inputABytes.Zip(inputBBytes, (a, b) => (byte)(a ^ b)).ToArray();

			Guid resultGuid = new Guid(resultBytes);

			return resultGuid;
		}

		public static string XorString(string inputA, string inputB)
		{
			if(string.IsNullOrWhiteSpace(inputA))
			{
				return inputB;
			}

			if (string.IsNullOrWhiteSpace(inputB))
			{
				return inputA;
			}

			List<ushort> charactersA = inputA.ToArray().Select(charValue => (ushort)charValue).ToList();
			List<ushort> charactersB = inputB.ToArray().Select(charValue => (ushort)charValue).ToList();

			int countA = charactersA.Count();
			int countB = charactersB.Count();

			if (countA > countB)
			{
				PadList(charactersB, countA - countB);
			}
			if (countB > countA)
			{
				PadList(charactersA, countB - countA);
			}

			IEnumerable<ushort> xorValue = charactersA.Zip(charactersB, (a, b) => (ushort)(a ^ b));

			char[] resultChars = xorValue.Select(ushortValue => (char)ushortValue).ToArray();

			string resultString = new string(resultChars);

			return resultString;
		}

		private static void PadList<CollectType>(List<CollectType> collection, int paddingCount)
		{
			for (int index = 0; index < paddingCount; index++)
			{
				collection.Add(default(CollectType));
			}
		}
	}
}
