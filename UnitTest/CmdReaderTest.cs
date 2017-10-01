using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CmdHost;

namespace UnitTest
{
	[TestClass]
	public class CmdReaderTest
	{
		public class MockReceiver : ICmdReceiver
		{
			public void AddData(string output)
			{
				Debug.WriteLine(output);
			}
		}

		public CmdReader NewReader()
		{
			var reader = new CmdReader();
			//reader.Register(new MockReceiver());
			return reader;
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
