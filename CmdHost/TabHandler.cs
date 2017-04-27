using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace CmdHost
{
	public class TabHandler
	{
		private readonly Terminal terminal;
		private int tabIndex;

		public string Dir { get; private set; } = "";

		public TabHandler(Terminal _terminal)
		{
			terminal = _terminal;
		}

		public void ResetTabComplete()
		{
			tabIndex = 0;
		}

		public bool IsTab(KeyEventArgs e)
		{
			if (e.Key == Key.Tab)
			{
				e.Handled = true;
				return HandleTab();
			}

			ResetTabComplete(); //Reset when input

			return false;
		}

		public bool HandleTab()
		{
			string Input = terminal.GetInput();

			string tabHit = ExtractFileName(Input);
			string AdditionalPath = SeperatePath(ref tabHit);

			try
			{
				string tabName = GetFile(AdditionalPath, tabHit);

				terminal.setInput(Input.Substring(0, Input.Length - tabHit.Length) + tabName);
			}
			catch (ArgumentException ex)
			{
				Debug.WriteLine(ex);
				ResetTabComplete();
			}

			return true;
		}

		private string GetFile(string AdditionalPath, string tabHit)
		{
			var files = Directory.GetFileSystemEntries(Dir + "\\" + AdditionalPath, tabHit + "*");

			if (files.Length == 0)
			{
				return "";
			}

			if (tabIndex >= files.Length)
			{
				ResetTabComplete();
			}

			string tabFile = files[tabIndex++];
			string tabName = tabFile.Substring(tabFile.LastIndexOf('\\') + 1);

			return tabName;
		}

		public string SeperatePath(ref string tabHit)
		{
			string AdditionalPath = "";

			if (tabHit.LastIndexOf('\\') != -1)
			{
				AdditionalPath += tabHit.Substring(0, tabHit.LastIndexOf('\\'));
				tabHit = tabHit.Substring(tabHit.LastIndexOf('\\') + 1);
			}

			return AdditionalPath;
		}

		public string ExtractFileName(string Input)
		{
			int pos = Input.LastIndexOf('"');
			if (pos == -1)
			{
				pos = Input.LastIndexOf(' ');
			}

			return Input.Substring(pos + 1);
		}

		public void ExtractDir(string outputs)
		{
			string lastLine = outputs.Substring(outputs.LastIndexOf('\n') + 1);

			if (Regex.IsMatch(lastLine, @"^\w:\\\S*>$"))
			{
				Dir = lastLine.Substring(0, lastLine.Length - 1);
			}
		}
	}
}