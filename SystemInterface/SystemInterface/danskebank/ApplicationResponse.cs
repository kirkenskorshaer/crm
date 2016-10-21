using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemInterface.DanskeBank
{
	public class ApplicationResponse
	{
		public string CustomerId;
		public DateTime Timestamp;
		public string ResponseCode;
		public string ResponseText;
		public string ExecutionSerial;
		public bool Encrypted;
		public bool Compressed;
		public string CompressionMethod;
		public List<FileDescriptor> FileDescriptors;
		public byte[] Content;
		public string Signature;
    }
}
