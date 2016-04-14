﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

namespace SystemInterface.Dynamics.Crm
{
	public class Annotation : AbstractCrm
	{
		public DateTime createdon;
		public DateTime modifiedon;
		public EntityReference modifiedby;
		public string notetext;

		private static readonly ColumnSet ColumnSetAnnotation = new ColumnSet(
		"annotationid",

		"notetext",

		"createdon",
		"modifiedon",
		"modifiedby"
		);

		protected override string entityName { get { return "annotation"; } }
		protected override string idName { get { return "annotationid"; } }

		private static readonly ColumnSet ColumnSetAnnotationCrmGenerated = new ColumnSet("createdon", "modifiedon", "modifiedby");
		protected override ColumnSet ColumnSetCrmGenerated { get { return ColumnSetAnnotationCrmGenerated; } }

		public Annotation(DynamicsCrmConnection connection) : base(connection)
		{
		}

		public Annotation(DynamicsCrmConnection connection, Entity annotationEntity) : base(connection, annotationEntity)
		{
		}

		protected override CrmEntity GetAsEntity(bool includeId)
		{
			CrmEntity crmEntity = new CrmEntity(entityName);

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
				crmEntity.Id = Id;
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("notetext", notetext));

			return crmEntity;
		}

		public Guid? modifiedbyGuid
		{
			get
			{
				if (modifiedby == null)
				{
					return null;
				}

				return modifiedby.Id;
			}
		}

		private static readonly DateTime _minimumSearchDate = new DateTime(1900, 1, 1);
		public static List<Annotation> ReadLatest(DynamicsCrmConnection connection, DateTime lastSearchDate, int? maximumNumberOfAnnotations = null)
		{
			List<Annotation> annotations = StaticCrm.ReadLatest(connection, "annotation", ColumnSetAnnotation, lastSearchDate, (lConnection, entity) => new Annotation(lConnection, entity), maximumNumberOfAnnotations);

			return annotations;
		}
	}
}
