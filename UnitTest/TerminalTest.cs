using System;
using System.Windows.Controls;
using CmdHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
	[TestClass]
	public class TerminalTest
	{
		private Terminal t;

		[TestInitialize]
		public void Init()
		{
			t = new Terminal(new TextBox());
		}

		[TestMethod]
		public void TestAppendOutput()
		{
			string msg = "Windows [version 10]\n2017\nc:\\>";
			t.AppendOutput(msg);

			Assert.AreEqual(msg.Length, t.DataLen);
			Assert.AreEqual(msg.Length, t.CaretIndex);
		}

		[TestMethod]
		public void AppendTextTest()
		{
			string msg = "Windows [version 10]\n2017\nc:\\>";
			t.AppendOutput(msg);

			string input = "net user";
			t.AppendText(input);

			Assert.AreEqual(msg.Length, t.DataLen);
			Assert.AreEqual(input, t.GetCmd());
		}

		[TestMethod]
		public void SetInputTest()
		{
			string msg = "Windows [version 10]\n2017\nc:\\>";
			t.AppendOutput(msg);

			t.AppendText("extra");
			string input = "net user";
			t.SetInput(input);

			Assert.AreEqual(msg.Length, t.DataLen);
			Assert.AreEqual(msg + input, t.Text);
		}
	}
}
