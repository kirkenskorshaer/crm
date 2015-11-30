using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace UserInterface.MongoData
{
	public class SimpleDataTabItem : TabItem
	{
		public string DataTypeName { get; set; }
		public DataLayer.MongoConnection MongoConnection { get; set; }

		public void RefreshData()
		{
			DataGrid dataGrid = new DataGrid();
			Content = dataGrid;

			List<Dictionary<string, object>> dataDictionaries = MongoConnection.ReadAsDictionaries(DataTypeName);
			List<string> keys = GetKeys(dataDictionaries);
			dataGrid.AutoGenerateColumns = false;

			keys.ForEach(key => dataGrid.Columns.Add(new DataGridTextColumn() { Header = key, Binding = new Binding(key) }));

			foreach (Dictionary<string, object> dataDictionary in dataDictionaries)
			{
				dynamic row = new ExpandoObject();

				foreach (string key in keys)
				{
					TextBlock block = new TextBlock();
					block.Text = "test";

					if (dataDictionary.ContainsKey(key))
					{
						((IDictionary<String, Object>)row)[key] = dataDictionary[key];
					}
				}
				dataGrid.Items.Add(row);
			}
		}

		private List<string> GetKeys(List<Dictionary<string, object>> dataDictionaries)
		{
			List<string> keys = new List<string>();
			foreach (Dictionary<string, object> dataDictionary in dataDictionaries)
			{
				foreach (string key in dataDictionary.Keys)
				{
					if (keys.Contains(key) == false)
					{
						keys.Add(key);
					}
				}
			}

			return keys;
		}
	}
}
