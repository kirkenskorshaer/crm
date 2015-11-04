using System.Collections.Generic;
using DataLayer;
using DataLayer.MongoData;
using NUnit.Framework;

namespace DataLayerTest.MongoDataTest
{
	[TestFixture]
	public class SignalTest
	{
		private MongoConnection _connection;
		private string optionName = "testOption";
		private string optionName2 = "testOption2";

		[SetUp]
		public void SetUp()
		{
			_connection = MongoConnection.GetConnection("test");
			_connection.CleanDatabase();
		}

		[Test]
		public void ReadSignalCreatesNewSignalNotStoredInDatabase()
		{
			List<Signal> signalsBefore = Signal.ReadSignalsByOption(_connection, optionName);

			Signal signal = Signal.ReadSignal(_connection, optionName, Signal.SignalTypeEnum.Fail);

			List<Signal> signalsAfter = Signal.ReadSignalsByOption(_connection, optionName);

			Assert.NotNull(signal);
			Assert.AreEqual(signalsBefore.Count, signalsAfter.Count);
		}

		[Test]
		public void IncreaseSignalCreatesSignalIfNeeded()
		{
			List<Signal> signalsBefore = Signal.ReadSignalsByOption(_connection, optionName);

			Signal.IncreaseSignal(_connection, optionName, Signal.SignalTypeEnum.Fail, 1d);

			List<Signal> signalsAfter = Signal.ReadSignalsByOption(_connection, optionName);
			Assert.AreEqual(signalsBefore.Count + 1, signalsAfter.Count);
		}

		[Test]
		public void IncreaseSignalIncreasesSignal()
		{
			Signal.IncreaseSignal(_connection, optionName, Signal.SignalTypeEnum.Fail, 1d);
			Signal signalBefore = Signal.ReadSignal(_connection, optionName, Signal.SignalTypeEnum.Fail);

			Signal.IncreaseSignal(_connection, optionName, Signal.SignalTypeEnum.Fail, 2d);

			Signal signalAfter = Signal.ReadSignal(_connection, optionName, Signal.SignalTypeEnum.Fail);
			Assert.AreEqual(signalBefore.Strength + 2d, signalAfter.Strength);
		}

		[Test]
		public void IncreaseSignalWithoutOptionIncreasesAllSignalsOfAType()
		{
			Signal.IncreaseSignal(_connection, optionName, Signal.SignalTypeEnum.Fail, 1d);
			Signal.IncreaseSignal(_connection, optionName2, Signal.SignalTypeEnum.Fail, 2d);

			Signal signalBefore = Signal.ReadSignal(_connection, optionName, Signal.SignalTypeEnum.Fail);
			Signal signalBefore2 = Signal.ReadSignal(_connection, optionName2, Signal.SignalTypeEnum.Fail);

			Signal.IncreaseSignal(_connection, Signal.SignalTypeEnum.Fail, 2d);

			Signal signalAfter = Signal.ReadSignal(_connection, optionName, Signal.SignalTypeEnum.Fail);
			Signal signalAfter2 = Signal.ReadSignal(_connection, optionName2, Signal.SignalTypeEnum.Fail);

			Assert.AreEqual(signalBefore.Strength + 2d, signalAfter.Strength);
			Assert.AreEqual(signalBefore2.Strength + 2d, signalAfter2.Strength);
		}

		[Test]
		public void IncreaseSignalDoesNotIncreasesSignalOfOtherTypes()
		{
			Signal.IncreaseSignal(_connection, optionName, Signal.SignalTypeEnum.Success, 1d);
			Signal signalBefore = Signal.ReadSignal(_connection, optionName, Signal.SignalTypeEnum.Success);

			Signal.IncreaseSignal(_connection, Signal.SignalTypeEnum.Fail, 2d);

			Signal signalAfter = Signal.ReadSignal(_connection, optionName, Signal.SignalTypeEnum.Success);
			Assert.AreEqual(signalBefore.Strength, signalAfter.Strength);
		}

		[Test]
		public void DecreaseAllDecreasesAll()
		{
			double factor = 2d;

			Signal.IncreaseSignal(_connection, optionName, Signal.SignalTypeEnum.Fail, 5d);
			Signal.IncreaseSignal(_connection, optionName, Signal.SignalTypeEnum.Success, 6d);
			Signal.IncreaseSignal(_connection, optionName2, Signal.SignalTypeEnum.Fail, 7d);
			Signal.IncreaseSignal(_connection, optionName2, Signal.SignalTypeEnum.Success, 10d);

			Signal signalBeforeFail = Signal.ReadSignal(_connection, optionName, Signal.SignalTypeEnum.Fail);
			Signal signalBeforeSuccess = Signal.ReadSignal(_connection, optionName, Signal.SignalTypeEnum.Success);
			Signal signalBeforeFail2 = Signal.ReadSignal(_connection, optionName2, Signal.SignalTypeEnum.Fail);
			Signal signalBeforeSuccess2 = Signal.ReadSignal(_connection, optionName2, Signal.SignalTypeEnum.Success);

			Signal.DecreaseAll(_connection, factor);

			Signal signalAfterFail = Signal.ReadSignal(_connection, optionName, Signal.SignalTypeEnum.Fail);
			Signal signalAfterSuccess = Signal.ReadSignal(_connection, optionName, Signal.SignalTypeEnum.Success);
			Signal signalAfterFail2 = Signal.ReadSignal(_connection, optionName2, Signal.SignalTypeEnum.Fail);
			Signal signalAfterSuccess2 = Signal.ReadSignal(_connection, optionName2, Signal.SignalTypeEnum.Success);

			Assert.AreEqual(signalBeforeFail.Strength / factor, signalAfterFail.Strength);
			Assert.AreEqual(signalBeforeSuccess.Strength / factor, signalAfterSuccess.Strength);
			Assert.AreEqual(signalBeforeFail2.Strength / factor, signalAfterFail2.Strength);
			Assert.AreEqual(signalBeforeSuccess2.Strength / factor, signalAfterSuccess2.Strength);
		}
	}
}
