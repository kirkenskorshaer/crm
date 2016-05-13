﻿using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.MongoData.Input
{
	public class WebCampaign
	{
		public ObjectId _id { get; set; }
		public Guid FormId;
		public string RedirectTarget;

		public void Insert(MongoConnection connection)
		{
			IMongoCollection<WebCampaign> webCampaignCollection = connection.Database.GetCollection<WebCampaign>(typeof(WebCampaign).Name);
			Task insertTask = webCampaignCollection.InsertOneAsync(this);
			MongoDataHelper.WaitForTaskOrThrowTimeout(insertTask);
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
	}
}