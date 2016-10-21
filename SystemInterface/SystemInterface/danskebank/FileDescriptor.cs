using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemInterface.DanskeBank
{
	public class FileDescriptor
	{
		public string FileReference;
		public string TargetId;
		public string ServiceId;
		public string UserFileName;
		public string ParentFileReference;
		public string FileType;
		public DateTime FileTimestamp;
		/// <summary>
		/// Only values DLD and NEW are used
		/// </summary>
		public string Status;

		public DateTime LastDownloadTimestamp;
		public string SubType;
    }
}
