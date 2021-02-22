using Microsoft.VisualStudio.TestTools.UnitTesting;
using CmdHost;

namespace UnitTest
{
	[TestClass]
	public class HistoryCmdTest
	{
		private HistoryCommand h;

		[TestInitialize]
		public void Init()
		{
			h = new HistoryCommand();

			h.Add("11");
			h.Add("222");
			h.Add("333");
		}

		[TestMethod]
		public void TestProp()
		{
			Assert.AreEqual(3, h.Size);
		}

		[TestMethod]
		public void TestUp()
		{
			Assert.AreEqual(h.SelectPrevious(), "333");

			Assert.AreEqual(h.SelectPrevious(), "222");

			Assert.AreEqual(h.SelectPrevious(), "11");

			Assert.AreEqual(h.SelectPrevious(), null);
		}

		[TestMethod]
		public void TestDown()
		{
			h.SelectPrevious();
			h.SelectPrevious();
			h.SelectPrevious();
			h.SelectPrevious();

			Assert.AreEqual(h.SelectNext(), "222");

			Assert.AreEqual(h.SelectNext(), "333");

			Assert.AreEqual(h.SelectNext(), null);
		}

		[TestMethod]
		public void TestUpAndDown()
		{
			Assert.AreEqual(h.SelectPrevious(), "333");

			Assert.AreEqual(h.SelectPrevious(), "222");

			Assert.AreEqual(h.SelectNext(), "333");
		}

		[TestMethod]
		public void NoDuplicate()
		{
			string c = h.GetItem(h.Size - 1);
			Assert.AreEqual(c, "333");

			h.Add(c);

			Assert.AreEqual(h.Size, 3);
		}
	}
}
