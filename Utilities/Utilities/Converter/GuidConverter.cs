using System;

namespace Utilities.Converter
{
	public static class GuidConverter
	{
		public static Guid Convert(int source1, int source2, int source3, int source4)
		{
			byte[] bytes = new byte[16];

			BitConverter.GetBytes(source1).CopyTo(bytes, 0);
			BitConverter.GetBytes(source2).CopyTo(bytes, 4);
			BitConverter.GetBytes(source3).CopyTo(bytes, 8);
			BitConverter.GetBytes(source4).CopyTo(bytes, 12);

			return new Guid(bytes);
		}
	}
}
