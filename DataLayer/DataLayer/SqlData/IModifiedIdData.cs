using System;

namespace DataLayer.SqlData
{
	public interface IModifiedIdData
	{
		Guid Id { get; }
		DateTime modifiedon { get; }
	}
}
