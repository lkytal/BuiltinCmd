using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using CmdHost;

namespace UnitTest
{
	internal class UIMock: ITerminalBoxProvider
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

	internal class TextBoxMock : ITextBoxSource
	{
		private readonly TextBox text;

		public TextBoxMock()
		{
			text = new TextBox();
		}

		public TextBox GetTextBox()
		{
			return text;
		}
	}
}
