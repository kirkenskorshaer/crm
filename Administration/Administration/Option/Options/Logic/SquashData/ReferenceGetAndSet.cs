using System;
using System.Collections.Generic;

namespace Administration.Option.Options.Logic.SquashData
{
	public class ReferenceGetAndSet
	{
		public Func<Guid, List<Guid>> GetReferences;
		public Action<List<Guid>> SetReferences;
	}
}
