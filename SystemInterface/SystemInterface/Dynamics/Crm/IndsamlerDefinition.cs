namespace SystemInterface.Dynamics.Crm
{
	public class IndsamlerDefinition
	{
		public int Aar;
		public IndsamlerTypeEnum IndsamlerType;
		public string IndsamlerRelationshipName = "new_account_contact_indsamlere";

		public enum IndsamlerTypeEnum
		{
			Indsamlingshjaelper = 1,
			Indsamler = 2,
		}
	}
}
