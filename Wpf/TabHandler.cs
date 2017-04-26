using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Wpf
{
	public class TabHandler
	{
		private readonly Controller controller;
		private int tabIndex;
		private int tabEnd;
		private string dir = "";

		public TabHandler(Controller _controller)
		{
			this.controller = _controller;
		}

		public void Reset(int RstLen)
		{
			tabIndex = 0;
			tabEnd = RstLen;
		}

		public void ResetTabComplete(Key key)
		{
			tabIndex = 0;

			switch (key)
			{
				case Key.Delete:
					tabEnd = controller.Rst.Text.Length - 1;
					break;
				case Key.Back:
					tabEnd = controller.Rst.Text.Length - 1;
					break;
				default:
					tabEnd = controller.Rst.Text.Length + 1;
					break;
			}
		}

		public bool HandleTab()
		{
			string cmd = controller.Rst.Text.Substring(controller.RstLen, tabEnd - controller.RstLen);

			int pos = cmd.LastIndexOf('"');
			if (pos == -1)
			{
				pos = cmd.LastIndexOf(' ');
			}

			string tabHit = cmd.Substring(pos + 1);

			try
			{
				string AdditionalPath = "\\";

				if (tabHit.LastIndexOf('\\') != -1)
				{
					AdditionalPath += tabHit.Substring(0, tabHit.LastIndexOf('\\'));
					tabHit = tabHit.Substring(tabHit.LastIndexOf('\\') + 1);
				}

				var files = Directory.GetFileSystemEntries(dir + AdditionalPath, tabHit + "*");

				if (files.Length == 0)
				{
					return true;
				}

				if (tabIndex >= files.Length)
				{
					tabIndex = 0;
				}

				controller.Rst.Text = controller.Rst.Text.Substring(0, tabEnd - tabHit.Length);

				string tabFile = files[tabIndex++];
				string tabName = tabFile.Substring(tabFile.LastIndexOf('\\') + 1);
				controller.Rst.AppendText(tabName);
				controller.Rst.Select(controller.Rst.Text.Length, 0);
			}
			catch (ArgumentException ex)
			{
				Debug.WriteLine(ex);
				tabIndex = 0;
			}

			return true;
		}

		public void ExtractDir(ref string outputs)
		{
			string lastLine = outputs.Substring(outputs.LastIndexOf('\n') + 1);

			if (Regex.IsMatch(lastLine, @"^\w:\\\S*>$"))
			{
				dir = lastLine.Substring(0, lastLine.Length - 1);
			}
		}
	}
}