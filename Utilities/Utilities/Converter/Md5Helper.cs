using System;
using System.Security.Cryptography;
using System.Text;

namespace Utilities.Converter
{
	public static class Md5Helper
	{
		public static string MakeMd5(string input)
		{
			string hash;
			using (MD5 md5 = MD5.Create())
			{
				hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(input)));
			}
			return hash;
		}
	}
}
