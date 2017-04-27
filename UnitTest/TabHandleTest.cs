using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CmdHost;

namespace UnitTest
{
	[TestClass]
	public class TabHandleTest
	{
		private TabHandler t;
		private readonly UI ui = new UIMock();

		[TestInitialize]
		public void Init()
		{
			t = new TabHandler(new Terminal(ui.GetTextBox()));
			t.ExtractDir(@"c:\>");
		}

		[TestMethod]
		public void TestExtractFile()
		{
			string input = @"dir net\local";

			string file = t.ExtractFileName(input);

			Assert.AreEqual(@"net\local", file);
		}

		[TestMethod]
		public void TestGetPath()
		{
			string tabHit = @"net\local";

			string path = t.SeperatePath(ref tabHit);

			Assert.AreEqual(@"net", path);
			Assert.AreEqual(@"local", tabHit);
		}

		const string output1 = "Microsoft Windows [version 10.0.15063]\n(c) 2017 Microsoft Corporation\nD:\\Code\\C#\\BuiltinCmd>";
		const string output2 = "Microsoft Windows [version 10.0.15063]\nD:\\Code\\C#\\>";
		const string output3 = "Microsoft Windows [version 10.0.15063]\nc:\\>";

		[TestMethod]
		[DataRow(output1)]
		[DataRow(output2)]
		[DataRow(output3)]
		public void TestExtractDir(string value)
		{
			string dir = value.Substring(value.LastIndexOf('\n') + 1);
			dir = dir.Remove(dir.Length - 1);

			t.ExtractDir(value);
			Assert.AreEqual(dir, t.Dir);
		}
	}
}
