using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

namespace SystemInterface.Dynamics.Crm
{
	public class Annotation : AbstractCrm, IComparable
	{
		public DateTime createdon;
		public DateTime modifiedon;
		public EntityReference modifiedby;
		public string notetext;

		public EntityReference objectid;
		public Guid? objectidReference
		{
			get { return GetEntityReferenceId(objectid); }
		}

		public void ObjectidSet(Guid value, string connectingEntity)
		{
			objectid = SetEntityReferenceId(value, connectingEntity);
		}

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

		public Annotation(IDynamicsCrmConnection connection) : base(connection)
		{
		}

		public Annotation(IDynamicsCrmConnection connection, Entity annotationEntity) : base(connection, annotationEntity)
		{
		}

		protected override Entity GetAsEntity(bool includeId)
		{
			Entity crmEntity = new Entity(entityName);

			if (includeId)
			{
				crmEntity.Attributes.Add(new KeyValuePair<string, object>(idName, Id));
				crmEntity.Id = Id;
			}

			crmEntity.Attributes.Add(new KeyValuePair<string, object>("notetext", notetext));
			crmEntity.Attributes.Add(new KeyValuePair<string, object>("objectid", objectid));

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
		public static List<Annotation> ReadLatest(IDynamicsCrmConnection connection, DateTime lastSearchDate, int? maximumNumberOfAnnotations = null)
		{
			List<Annotation> annotations = StaticCrm.ReadLatest(connection, "annotation", ColumnSetAnnotation, lastSearchDate, (lConnection, entity) => new Annotation(lConnection, entity), maximumNumberOfAnnotations);

			return annotations;
		}

		public int CompareTo(object obj)
		{
			Annotation compareAnnotation = obj as Annotation;

			return compareAnnotation.notetext.CompareTo(notetext);
		}
	}
}
