using System;
using System.Diagnostics;
using System.IO;
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
		[DataRow(@"verify", @"verify")]
		[DataRow(@"dir net", @"net")]
		[DataRow(@"dir net\local", @"net\local")]
		[DataRow(@"dir net\local\test", @"net\local\test")]
		[DataRow(@"dir /L net\local", @"net\local")]
		public void TestExtractFileName(string input, string expected)
		{
			Assert.AreEqual(expected, t.ExtractFileName(input));
		}

		[TestMethod]
		[DataRow(@"local", @"", @"local")]
		[DataRow(@"net\local", @"net", @"local")]
		[DataRow(@"net\local\test", @"net\local", @"test")]
		public void TestSeperatePath(ref string input, string expectPath, string expectHit)
		{
			string path = t.SeperatePath(ref input);

			Assert.AreEqual(expectPath, path);
			Assert.AreEqual(input, expectHit);
		}

		[TestMethod]
		[DataRow("Microsoft Windows [version 10.0.15063]\n(c) 2017 Microsoft Corporation\nD:\\Code\\C#\\BuiltinCmd>")]
		[DataRow("Microsoft Windows [version 10.0.15063]\nD:\\Code\\C#\\>")]
		[DataRow("Microsoft Windows [version 10.0.15063]\nc:\\>")]
		public void TestExtractDir(string value)
		{
			string dir = value.Substring(value.LastIndexOf('\n') + 1);
			dir = dir.Remove(dir.Length - 1);

			t.ExtractDir(value);
			Assert.AreEqual(dir, t.Dir);
		}

		[TestMethod]
		[DataRow("", "", 0, "file.txt")]
		[DataRow("", "", 1, "w")]
		[DataRow("", "", 2, "wpf.exe")]
		[DataRow("", "w", 0, "w")]
		[DataRow("", "w", 1, "wpf.exe")]
		[DataRow("", "w", 2, "wpf.exe.json")]
		[DataRow("", "", 100, "file.txt")] //over index

		[DataRow("zero", "", 0, "file.txt")]
		[DataRow("zero", "", 1, "w")]
		[DataRow("zero", "", 2, "wpf.exe")]
		[DataRow("zero", "w", 0, "w")]
		[DataRow("zero", "w", 1, "wpf.exe")]
		[DataRow("zero", "w", 2, "wpf.exe.json")]
		public void TestGetFile(string addtionalPath, string tabHit, int index, string expectFile)
		{
			string path = Directory.GetCurrentDirectory();
			path = Directory.GetParent(Directory.GetParent(path).ToString()) + @"\TestDir";
			t.ExtractDir(path + @"\>");
			//t.ResetTabComplete();
			Debug.WriteLine(path);

			Assert.AreEqual(expectFile, t.GetFile(addtionalPath, tabHit, index));
		}

		[TestMethod]
		[DataRow("", 0, "file.txt")]
		[DataRow("", 1, "w")]
		[DataRow("", 100, "file.txt")] //over index

		[DataRow("w", 0, "w")]
		[DataRow("w", 1, "wpf.exe")]
		[DataRow("w", 2, "wpf.exe.json")]

		[DataRow("dir ", 0, "dir file.txt")]
		[DataRow("dir ", 1, "dir w")]
		[DataRow("dir w", 0, "dir w")]
		[DataRow("dir w", 1, "dir wpf.exe")]

		[DataRow("dir /L ", 0, "dir /L file.txt")]
		[DataRow("dir /L ", 1, "dir /L w")]
		[DataRow("dir /L w", 0, "dir /L w")]
		[DataRow("dir /L w", 1, "dir /L wpf.exe")]

		[DataRow(@"dir zero\", 0, @"dir zero\file.txt")]
		[DataRow(@"dir zero\", 1, @"dir zero\w")]
		[DataRow(@"dir zero\w", 0, @"dir zero\w")]
		[DataRow(@"dir zero\w", 1, @"dir zero\wpf.exe")]

		public void TestCompleteInput(string input, int index, string expected)
		{
			string path = Directory.GetCurrentDirectory();
			path = Directory.GetParent(Directory.GetParent(path).ToString()) + @"\TestDir";
			t.ExtractDir(path + @"\>");
			t.ResetTabComplete();
			Debug.WriteLine(path);

			Assert.AreEqual(expected, t.CompleteInput(input, index));
		}
	}
}
