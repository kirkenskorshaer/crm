using System;
using System.Threading.Tasks;

namespace DataLayer.MongoData
{
	public static class MongoDataHelper
	{
		internal static SearchedObjectType GetValueOrThrowTimeout<SearchedObjectType>(Task<SearchedObjectType> task)
		{
			int timeoutMilliSeconds = MongoConnection.TimeoutMilliSeconds;

			task.Wait(timeoutMilliSeconds);

			if (task.IsCompleted == false)
			{
				throw new TimeoutException($"timeout after waiting {timeoutMilliSeconds} MilliSeconds to fetch {typeof(SearchedObjectType).Name}");
			}

			return task.Result;
		}
	}
}
