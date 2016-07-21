using System.Collections.Generic;

namespace SystemInterface.Mailrelay.Function.LogFunctions
{
	public class getMailRcptInfoReply
	{
		public string type;
		public string timeLogged;
		public string timeQueued;
		public string timeImpinted;
		public string orig;
		public string rcpt;
		public string orcpt;
		public string dsnAction;
		public string dsnStatus;
		public string dsnDiag;
		public string dsnMta;
		public string bounceCat;
		public string srcType;
		public string srcMta;
		public string dlvType;
		public string dlvSourceIp;
		public string dlvDestinationIp;
		public string dlvEsmtpAvailable;
		public string dlvSize;
		public string vmta;
		public int? jobId;
		public int? envId;
		public int? sentId;
		public int? mailingListId;
	}
}
