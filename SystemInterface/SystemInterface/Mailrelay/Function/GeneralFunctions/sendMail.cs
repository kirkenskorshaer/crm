using System;
using System.Collections.Generic;
using SystemInterface.Mailrelay.FunctionReply;

namespace SystemInterface.Mailrelay.Function.SMTP
{
	public class sendMail : AbstractFunction
	{
		public override Type ReturnType { get { return typeof(MailrelayBoolReply); } }

		///<summery>Array with all recipients in the following format:
		/// array( 'name' => 'Name 1', 'email' => '' ), array( 'name' => 'Name 1', 'email' => '' )
		/// Name is optional and email is required. If an invalid email is provided it will be automatically ignored.</summery>
		public Dictionary<string, string> emails;

		///<summery>Subject of the message.</summery>
		public string subject;

		///<summery>Html of the message. There is no need to provide plain text as it is automatically generated based on the provided html.</summery>
		public string html;

		///<summery>Id of the mailbox that will be used on the From header. You can get all mailboxes using function.</summery>
		public int mailboxFromId;

		///<summery>Id of the mailbox that will be used on the Reply-To header. You can get all mailboxes using function.</summery>
		public int mailboxReplyId;

		///<summery>Mailbox report id. You can get all mailboxes using function.</summery>
		public int mailboxReportId;

		///<summery>Id of the package used to send this email. You can get all packages using function.</summery>
		public int packageId;

		///<summery>Optional parameter in case you want to add attachments. Its elements should follow this structure</summery>
		public List<string> attachments;
	}
}
