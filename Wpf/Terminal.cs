using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
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
			this.Rst = _textBox;
		}

		public void AppendText(string text)
		{
			Rst.AppendText(text);
			this.DataLen = Rst.Text.Length;
			FocusEnd();
		}

		public void FocusEnd()
		{
			Rst.Select(Rst.Text.Length, 0);
		}
	}
}
