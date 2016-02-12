﻿using System;

namespace DataLayer.MongoData.Option.Options.Logic
{
	public class SynchronizeFromCsv : OptionBase
	{
		public string fileName { get; set; }
		public string fileNameTmp { get; set; }
		public char delimeter { get; set; }
		public string keyName { get; set; }
		public string dateName { get; set; }
		public string mappingField { get; set; }
		public string[] fields { get; set; }

		public Guid changeProviderId { get; set; }

		public static SynchronizeFromCsv Create(MongoConnection connection, string name, Schedule schedule, Guid changeProviderId, string fileName, string fileNameTmp, char delimeter, string keyName,string dateName, string mappingField, string[] fields)
		{
			SynchronizeFromCsv synchronizeFromCsv = new SynchronizeFromCsv
			{
				fileName = fileName,
				changeProviderId = changeProviderId,
				dateName = dateName,
				delimeter = delimeter,
				fields = fields,
				fileNameTmp = fileNameTmp,
				keyName = keyName,
				mappingField = mappingField,
			};

			Create(connection, synchronizeFromCsv, name, schedule);

			return synchronizeFromCsv;
		}

		protected override void Execute(MongoConnection connection, bool recurring)
		{
			if (recurring)
			{
				Update<SynchronizeFromCsv>(connection);
			}
			else
			{
				Delete<SynchronizeFromCsv>(connection);
			}
		}
	}
}
