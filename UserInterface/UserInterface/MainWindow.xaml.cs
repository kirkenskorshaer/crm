using DataLayer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using UserInterface.MongoData;

namespace UserInterface
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			PopulateTabControlGlobal();
		}

		private void PopulateTabControlGlobal()
		{
			string mongoDataNamespaceName = "DataLayer.MongoData";
			string optionNamespaceName = "DataLayer.MongoData.Option.Options";

			string databaseName = ConfigurationManager.AppSettings["mongoDatabaseName"];
			MongoConnection mongoConnection = MongoConnection.GetConnection(databaseName);

			Assembly assembly = Assembly.GetAssembly(typeof(MongoConnection));

			List<Type> allMongoTypes = assembly.GetTypes().Where(type =>
				type.IsDefined(typeof(CompilerGeneratedAttribute)) == false &&
				type.IsClass &&
				type.Namespace.StartsWith(mongoDataNamespaceName)).ToList();

			List<Type> simpleStructureDataTypes = allMongoTypes.Where(type => type.Namespace == mongoDataNamespaceName).ToList();
			List<Type> optionDataTypes = allMongoTypes.Where(type => type.Namespace.StartsWith(optionNamespaceName)).ToList();

			foreach (Type type in simpleStructureDataTypes)
			{
				SimpleDataTabItem tabItem = new SimpleDataTabItem()
				{
					Header = type.Name,
					DataTypeName = type.Name,
					MongoConnection = mongoConnection,
				};

				tabItem.RefreshData();

				TabControlGlobal.Items.Add(tabItem);
			}

			TabItem optionsTabItem = new TabItem();
			optionsTabItem.Header = "Options";

			TabControl optionsControl = new TabControl();
			optionsTabItem.Content = optionsControl;

			TabControlGlobal.Items.Add(optionsTabItem);

			foreach (Type type in optionDataTypes)
			{
				SimpleDataTabItem tabItem = new SimpleDataTabItem()
				{
					Header = type.Name,
					DataTypeName = type.Name,
					MongoConnection = mongoConnection,
				};

				tabItem.RefreshData();

				optionsControl.Items.Add(tabItem);
			}
		}
	}
}
