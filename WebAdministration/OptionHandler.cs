using DataLayer;
using DataLayer.MongoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using WebAdministration.Option;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;

namespace WebAdministration
{
	public class OptionHandler
	{
		MongoConnection _mongoConnection;

		public OptionHandler(MongoConnection mongoConnection)
		{
			_mongoConnection = mongoConnection;
		}

		public Dictionary<string, object> GetResponse(DatabaseOptionBase databaseOption)
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();

			OptionBase option = null;
			try
			{
				option = GetOption(databaseOption);

				if (option == null)
				{
					return parameters;
				}

				return option.ExecuteOption();
			}
			catch (Exception exception)
			{
				Log.Write(_mongoConnection, exception.Message, typeof(OptionHandler), exception.StackTrace, Config.LogLevelEnum.OptionError);
			}

			return parameters;
		}

		public OptionBase GetOption(DatabaseOptionBase databaseOption)
		{
			List<Type> optionTypes = ReflectionHelper.GetChildTypes(typeof(OptionBase));

			Type matchingOptionType = optionTypes.SingleOrDefault(optionType => optionType.Name == databaseOption.GetType().Name);

			if (matchingOptionType == null)
			{
				Log.Write(_mongoConnection, $"Unknown option type {matchingOptionType}", typeof(OptionHandler), Config.LogLevelEnum.OptionError);
				return null;
			}

			OptionBase option = (OptionBase)Activator.CreateInstance(matchingOptionType, _mongoConnection, databaseOption);

			return option;
		}
	}
}
