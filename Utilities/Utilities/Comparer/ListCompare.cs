using System;
using System.Collections.Generic;

namespace Utilities.Comparer
{
	public static class ListCompare
	{
		public static bool ListEquals<ListObjectType>(List<ListObjectType> list1, List<ListObjectType> list2)
		where ListObjectType : class
		{
			return ListEquals(list1, list2, (a, b) => a == b);
		}

		public static bool ListEquals(List<Guid> list1, List<Guid> list2)
		{
			return ListEquals(list1, list2, (a, b) => a == b);
		}

		public static bool ListEquals<ListType1, ListType2>(List<ListType1> list1, List<ListType2> list2, Func<ListType1, ListType2, bool> isEqual)
		{
			if (list1 == null && list2 == null)
			{
				return true;
			}

			if (list1 == null || list2 == null)
			{
				return false;
			}

			if (list1.Count != list2.Count)
			{
				return false;
			}

			list1.Sort();
			list2.Sort();

			for (int idIndex = 0; idIndex < list1.Count; idIndex++)
			{
				if (isEqual(list1[idIndex], list2[idIndex]) == false)
				{
					return false;
				}
			}

			return true;
		}
	}
}
