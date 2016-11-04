using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataLayer.MongoData.Input
{
	public class WebCampaign : AbstractMongoData
	{
		[BsonRepresentation(BsonType.String)]
		public Guid FormId;
		[BsonRepresentation(BsonType.String)]
		public Guid FormOwner;
		public string RedirectTarget;
		public List<string> KeyFields;
		public CollectTypeEnum CollectType;
		public int? mailrelaygroupid;

		public enum CollectTypeEnum
		{
			Lead = 1,
			LeadOgContactHvisContactIkkeFindes = 2,
		}

		public void Insert(MongoConnection connection)
		{
			Create(connection, this);
		}

		public static WebCampaign ReadSingleOrDefault(MongoConnection connection, Guid formId)
		{
			IMongoCollection<WebCampaign> WebCampaignCollection = connection.Database.GetCollection<WebCampaign>(typeof(WebCampaign).Name);
			IFindFluent<WebCampaign, WebCampaign> WebCampaignFind = WebCampaignCollection.Find(WebCampaign =>
				WebCampaign.FormId == formId);

			Task<List<WebCampaign>> WebCampaignTask = WebCampaignFind.ToListAsync();

			WebCampaignTask.Wait();

			List<WebCampaign> WebCampaignFound = WebCampaignTask.Result;

			return WebCampaignFound.SingleOrDefault();
		}

		public static WebCampaign ReadByIdBytesSingleOrDefault(MongoConnection connection, byte[] id)
		{
			ObjectId searchId = new ObjectId(id);
			return ReadByIdSingleOrDefault(connection, searchId);
		}

		public static WebCampaign ReadByIdSingleOrDefault(MongoConnection connection, ObjectId id)
		{
			IMongoCollection<WebCampaign> WebCampaignCollection = connection.Database.GetCollection<WebCampaign>(typeof(WebCampaign).Name);
			IFindFluent<WebCampaign, WebCampaign> WebCampaignFind = WebCampaignCollection.Find(WebCampaign =>
				WebCampaign._id == id);

			Task<List<WebCampaign>> WebCampaignTask = WebCampaignFind.ToListAsync();

			WebCampaignTask.Wait();

			List<WebCampaign> WebCampaignFound = WebCampaignTask.Result;

			return WebCampaignFound.SingleOrDefault();
		}

		public void Update(MongoConnection connection)
		{
			Update<WebCampaign>(connection);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			WebCampaign compareWebCampaign = obj as WebCampaign;

			return
				CollectType == compareWebCampaign.CollectType &&
				FormId == compareWebCampaign.FormId &&
				FormOwner == compareWebCampaign.FormOwner &&
				Utilities.Comparer.ListCompare.ListEquals(KeyFields, compareWebCampaign.KeyFields, (a, b) => a.Equals(b)) &&
				RedirectTarget == compareWebCampaign.RedirectTarget &&
				mailrelaygroupid == compareWebCampaign.mailrelaygroupid;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
