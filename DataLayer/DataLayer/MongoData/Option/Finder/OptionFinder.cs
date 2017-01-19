using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Utilities;

namespace DataLayer.MongoData.Option.Finder
{
	public class OptionFinder
	{
		private readonly MongoConnection _connection;
		public OptionFinder(MongoConnection connection)
		{
			_connection = connection;
		}

		public List<OptionBase> Find()
		{
			return Find(null);
		}

		public List<OptionBase> Find(Worker worker)
		{
			List<Type> optionTypes = ReflectionHelper.GetChildTypes(typeof(OptionBase));

			List<OptionBase> options = new List<OptionBase>();

			foreach (Type optionType in optionTypes)
			{
				MethodInfo method = typeof(OptionBase).GetMethod("ReadAllowed");
				MethodInfo generic = method.MakeGenericMethod(optionType);

				IEnumerable optionsOnCurrentType = null;

				optionsOnCurrentType = (IEnumerable)generic.Invoke(null, new object[] { _connection, worker });

				foreach (object option in optionsOnCurrentType)
				{
					options.Add((OptionBase)option);
				}
			}

			return options;
		}
	}
}
