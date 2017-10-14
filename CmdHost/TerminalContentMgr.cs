using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace CmdHost
{
	public interface ITextBoxSource
	{
		TextBox GetTextBox();
	}

	public class TerminalContentMgr
	{
		private readonly ITextBoxSource textControl;
		private TextBox Rst => textControl.GetTextBox();

		public int DataLen { get; private set; }
		public int Length => Rst.Text.Length;
		public string Text { get => Rst.Text; set => Rst.Text = value; }
		public int CaretIndex => Rst.CaretIndex;

		private string currentInput = "";

		public TerminalContentMgr(ITextBoxSource textControl)
		{
			this.textControl = textControl;

			Rst.KeyUp += (s, e) =>
			{
				if (e.Key != Key.Tab)
				{
					currentInput = Text.Substring(DataLen, Text.Length - DataLen);
				}
			};
		}

		public void AppendOutput(string text)
		{
			Rst.AppendText(text);
			DataLen = Rst.Text.Length;
			FocusEnd();
		}

		public void FocusEnd()
		{
			Rst.Select(Rst.Text.Length, 0);
		}

		public void Clear()
		{
			Rst.Clear();
			DataLen = 0;
		}

		public string GetCmd()
		{
			return Text.Substring(DataLen, Text.Length - DataLen);
		}

		public string Substring(int start, int length)
		{
			return Text.Substring(start, length);
		}

		public void AppendText(string text)
		{
			Rst.AppendText(text);
		}

		public void SetInput(string input)
		{
			if (input != null)
			{
				Text = Text.Substring(0, DataLen) + input;
				FocusEnd();
			}
		}

		public string GetInput()
		{
			return currentInput;
		}

		public void RemoveInput()
		{
			Text = Text.Substring(0, DataLen);
		}
	}
}
