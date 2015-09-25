using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace DataLayer.SqlData
{
	public class Contact
	{
		public Guid ContactId;
		public DateTime CreatedOn;
		public DateTime ModifiedOn;
		public string Firstname;
		public string Lastname;
	}
}
