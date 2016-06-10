using System;

namespace SystemInterface.Dynamics.Crm
{
	public class MaterialeProcessState
	{
		public MaterialeBehovDefinition.behovtypeEnum BehovType = MaterialeBehovDefinition.behovtypeEnum.Indsamlingssted;
		public int AccountsProcessed = 0;
		public PagingInformation pagingInformation = null;
	}
}
