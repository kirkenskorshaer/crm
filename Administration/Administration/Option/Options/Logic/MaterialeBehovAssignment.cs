using Administration.Option.Options.Data;
using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseOptionBase = DataLayer.MongoData.Option.OptionBase;
using DatabaseMaterialeBehovAssignment = DataLayer.MongoData.Option.Options.Logic.MaterialeBehovAssignment;
using DatabaseUrlLogin = DataLayer.MongoData.UrlLogin;
using SystemInterface.Dynamics.Crm;

namespace Administration.Option.Options.Logic
{
	public class MaterialeBehovAssignment : AbstractDataOptionBase
	{
		private DatabaseMaterialeBehovAssignment _databaseMaterialeBehovAssignment;

		public MaterialeBehovAssignment(MongoConnection connection, DatabaseOptionBase databaseOption) : base(connection, databaseOption)
		{
			_databaseMaterialeBehovAssignment = (DatabaseMaterialeBehovAssignment)databaseOption;
		}

		public static List<MaterialeBehovAssignment> Find(MongoConnection connection)
		{
			List<DatabaseMaterialeBehovAssignment> options = DatabaseOptionBase.ReadAllowed<DatabaseMaterialeBehovAssignment>(connection);

			return options.Select(option => new MaterialeBehovAssignment(connection, option)).ToList();
		}

		protected override bool ExecuteOption()
		{
			string urlLoginName = _databaseMaterialeBehovAssignment.urlLoginName;

			DatabaseUrlLogin login = DatabaseUrlLogin.GetUrlLogin(Connection, urlLoginName);
			DynamicsCrmConnection dynamicsCrmConnection = DynamicsCrmConnection.GetConnection(login.Url, login.Username, login.Password);

			Materiale materiale = Materiale.ReadCalculationNeed(dynamicsCrmConnection, Config.GetResourcePath);

			if (materiale == null)
			{
				return true;
			}

			materiale.FindBehovDefinitioner(Config.GetResourcePath);

			int total = CountTotalAccountsToUpdate(dynamicsCrmConnection, materiale);

			materiale.new_beregningsstatus = 0;
			materiale.Update();

			materiale.RemoveStaleMaterialeBehov(Config.GetResourcePath);

			int materialeAdded = 0;
			int materialeAddedCurrent = -1;

			MaterialeProcessState state = new MaterialeProcessState();
			state.owningbusinessunit = materiale.owningbusinessunitGuid.Value;

			while (materialeAddedCurrent != 0)
			{
				materiale.AddMissingMateriale(state, Config.GetResourcePath);
				materialeAddedCurrent = state.AccountsProcessed;
				materialeAdded += materialeAddedCurrent;
				updateProgress(materiale, materialeAdded, total);
			}
			materialeAddedCurrent = -1;

			state.withMaterialeBehov = false;
			state.BehovType = MaterialeBehovDefinition.behovtypeEnum.Indsamlingssted;

			while (materialeAddedCurrent != 0)
			{
				materiale.AddMissingMateriale(state, Config.GetResourcePath);
				materialeAddedCurrent = state.AccountsProcessed;
				materialeAdded += materialeAddedCurrent;
				updateProgress(materiale, materialeAdded, total);
			}

			materiale.behovsberegning = Materiale.behovsberegningEnum.Afsluttet;
			materiale.new_beregningsstatus = 0;
			materiale.Update();

			return true;
		}

		private void updateProgress(Materiale materiale, int materialeAdded, int total)
		{
			int progress = 0;
			if (total != 0)
			{
				progress = ((materialeAdded * 100) / total);
			}

			if (progress < 0)
			{
				progress = 0;
			}

			if (progress > 99)
			{
				progress = 99;
			}

			materiale.new_beregningsstatus = progress;
			materiale.Update();
		}

		private int CountTotalAccountsToUpdate(DynamicsCrmConnection dynamicsCrmConnection, Materiale materiale)
		{
			int total = 0;

			if (materiale.BehovDefinitioner.Any(definition => definition.behovtype == MaterialeBehovDefinition.behovtypeEnum.ForventetAntalIndsamlere2016))
			{
				total += Account.CountForventetAntalIndsamlere2016(dynamicsCrmConnection, Config.GetResourcePath);
			}

			if (materiale.BehovDefinitioner.Any(definition => definition.behovtype == MaterialeBehovDefinition.behovtypeEnum.Indsamlingshjaelper))
			{
				total += Account.CountIndsamlingshjaelper(dynamicsCrmConnection, Config.GetResourcePath);
			}

			if (materiale.BehovDefinitioner.Any(definition => definition.behovtype == MaterialeBehovDefinition.behovtypeEnum.Indsamlingssted))
			{
				total += Account.CountIndsamlingssteder(dynamicsCrmConnection, Config.GetResourcePath);
			}

			return total;
		}
	}
}
