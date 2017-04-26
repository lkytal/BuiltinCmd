using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wpf;

namespace UnitTest
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			var app = new MainWindow();
			Assert.IsNotNull(app);
		}
	}
}
