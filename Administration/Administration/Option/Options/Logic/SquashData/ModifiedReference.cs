using System;
using System.Collections.Generic;

namespace Administration.Option.Options.Logic.SquashData
{
	public class ModifiedReference
	{
		public List<Guid> RelationIdsAdded;
		public List<Guid> RelationIdsRemoved;
		public Type RelationType;
		public DateTime ModifiedOn;
	}
}
