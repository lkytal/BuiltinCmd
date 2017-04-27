using System;
using System.Windows.Controls;

namespace Wpf
{
	public class Terminal
	{
		public readonly TextBox Rst;
		public int DataLen { get; set; }

		public int Length => Rst.Text.Length;

		public string Text { get => Rst.Text; set => Rst.Text = value; }

		public Terminal(TextBox _textBox)
		{
			Rst = _textBox;
		}

		public void AppendText(string text)
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

		public string GetInput()
		{
			return Text.Substring(DataLen, Text.Length - DataLen);
		}

		public string Substring(int start, int length)
		{
			return Text.Substring(start, length);
		}
	}
}
