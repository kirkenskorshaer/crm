using System;
using SystemInterface.Dynamics.Crm;

namespace Administration.Option.Options.Logic.SumIndbetalingData
{
	public class IndbetalingSumPart
	{
		public Guid Id { get; private set; }
		public Indbetaling.kildeEnum? Kilde { get; private set; }
		public decimal Amount = 0;

		public IndbetalingSumPart(Guid id, Indbetaling.kildeEnum? kilde)
		{
			Id = id;
			Kilde = kilde;
		}
	}
}
