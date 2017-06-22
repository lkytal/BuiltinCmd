using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CmdHost;

namespace UnitTest
{
	[TestClass]
	public class CmdReaderTest
	{
		public class MockReceiver : CmdReceiver
		{
			public void AddData(string output)
			{
				Debug.WriteLine(output);
			}
		}

		public CmdReader NewReader()
		{
			return new CmdReader(new MockReceiver());
		}

		[TestMethod]
		public void TestCreate()
		{
			CmdReader r = NewReader();
			Assert.IsNotNull(r);

			Assert.IsTrue(r.Init());
		}
	}
}
