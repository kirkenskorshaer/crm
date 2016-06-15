using System;

namespace SystemInterface.Dynamics.Crm
{
	public class MaterialeProcessState
	{
		public bool withMaterialeBehov = true;
		public Guid LargestAccountId = Guid.Empty;
		public MaterialeBehovDefinition.behovtypeEnum BehovType = MaterialeBehovDefinition.behovtypeEnum.Indsamlingssted;
		public int AccountsProcessed = 0;
		public Guid owningbusinessunit;
    }
}
