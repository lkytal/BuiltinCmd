using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Wpf
{
	public class TabHandler
	{
		private readonly Terminal terminal;
		private int tabIndex;
		private string dir = "";

		public TabHandler(Terminal _terminal)
		{
			terminal = _terminal;
		}

		public void ResetTabComplete()
		{
			tabIndex = 0;
		}

		public bool HandleTab(string Input)
		{
			string cmd = Input;

			int pos = cmd.LastIndexOf('"');
			if (pos == -1)
			{
				pos = cmd.LastIndexOf(' ');
			}

			string tabHit = cmd.Substring(pos + 1);

			try
			{
				string AdditionalPath = "";

				if (tabHit.LastIndexOf('\\') != -1)
				{
					AdditionalPath += tabHit.Substring(0, tabHit.LastIndexOf('\\'));
					tabHit = tabHit.Substring(tabHit.LastIndexOf('\\') + 1);
				}

				var files = Directory.GetFileSystemEntries(dir + "\\" + AdditionalPath, tabHit + "*");

				if (files.Length == 0)
				{
					return true;
				}

				if (tabIndex >= files.Length)
				{
					tabIndex = 0;
				}

				terminal.Text = terminal.Substring(0, terminal.DataLen + Input.Length - tabHit.Length);

				string tabFile = files[tabIndex++];
				string tabName = tabFile.Substring(tabFile.LastIndexOf('\\') + 1);
				terminal.Rst.AppendText(tabName);
				terminal.FocusEnd();
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