using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using CmdHost;

namespace UnitTest
{
	internal class UIMock: UI
	{
		public Dispatcher Dispatcher => Dispatcher.CurrentDispatcher;
		private readonly TextBox text;

		public UIMock()
		{
			text = new TextBox();
		}

		public TextBox GetTextBox()
		{
			return text;
		}
	}

	internal class TextBoxMock : TextBoxSource
	{
		private readonly TextBox text = new TextBox();

		public TextBox GetTextBox()
		{
			return text;
		}
	}
}
