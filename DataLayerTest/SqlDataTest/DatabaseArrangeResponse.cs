using DataLayer.SqlData;
using DataLayer.SqlData.Account;
using DataLayer.SqlData.Contact;
using DataLayer.SqlData.Group;

namespace DataLayerTest.SqlDataTest
{
	internal class DatabaseArrangeResponse
	{
		internal AccountChangeIndsamler AccountChangeIndsamler;
		internal AccountChangeContact AccountChangeContact;
		internal Account Account;
		internal Contact Contact;
		internal ExternalAccount ExternalAccount;
		internal AccountChange AccountChange;
		internal AccountChangeGroup AccountChangeGroup;
		internal ChangeProvider ChangeProvider;
		internal ExternalContact ExternalContact;
		internal ContactChange ContactChange;
		internal Group Group;
		internal ContactChangeGroup ContactChangeGroup;
	}
}
