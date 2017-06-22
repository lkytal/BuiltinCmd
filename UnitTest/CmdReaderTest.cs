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

		[TestMethod]
		public void TestMethod1()
		{
		}
	}
}
