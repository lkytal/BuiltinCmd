using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace CmdHost
{
	public class Terminal
	{
		private readonly TextBox Rst;

		public int DataLen { get; private set; }
		public int Length => Rst.Text.Length;
		public string Text { get => Rst.Text; set => Rst.Text = value; }
		public int CaretIndex => Rst.CaretIndex;

		private string Input = "";

		public Terminal(TextBox _textBox)
		{
			Rst = _textBox;

			Rst.KeyUp += (s, e) =>
			{
				if (e.Key != Key.Tab)
				{
					Input = Text.Substring(DataLen, Text.Length - DataLen);
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
			Rst.Text = "";
			DataLen = 0;
		}

		public void removeInput()
		{
			Text = Text.Substring(0, DataLen);
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

		public void setInput(string input)
		{
			if (input != null)
			{
				Text = Text.Substring(0, DataLen) + input;
				FocusEnd();
			}
		}

		public string GetInput()
		{
			return Input;
		}
	}
}
