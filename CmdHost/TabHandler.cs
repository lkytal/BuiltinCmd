using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace CmdHost
{
	public class TabHandler
	{
		private readonly Terminal terminal;

		private int TabIndex { get; set; } = 0;
		public string Dir { get; private set; } = "";

		public TabHandler(Terminal _terminal)
		{
			terminal = _terminal;
		}

		public void ResetTabComplete()
		{
			TabIndex = 0;
		}

		public void HandleTab()
		{
			try
			{
				string completedLine = CompleteInput(terminal.GetInput(), TabIndex);
				terminal.SetInput(completedLine);
				TabIndex += 1;
			}
			catch (Exception)
			{
				ResetTabComplete();
			}
		}

		public string CompleteInput(string input, int index)
		{
			string tabHit = ExtractFileName(input);
			string AdditionalPath = SeperatePath(ref tabHit);

			string tabName = GetFile(AdditionalPath, tabHit, index);

			return input.Substring(0, input.Length - tabHit.Length) + tabName;
		}

		public string GetFile(string additionalPath, string tabHit, int index)
		{
			var files = Directory.GetFileSystemEntries(Dir + "\\" + additionalPath, tabHit + "*");

			if (files.Length == 0)
			{
				return "";
			}

			if (index >= files.Length)
			{
				ResetTabComplete();
				index = 0;
			}

			string tabFile = files[index];
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

		public string ExtractFileName(string input)
		{
			int pos = input.LastIndexOf('"');
			if (pos == -1)
			{
				pos = input.LastIndexOf(' ');
			}

			return input.Substring(pos + 1);
		}

		public void ExtractDir(string outputs)
		{
			string lastLine = outputs.Substring(outputs.LastIndexOf('\n') + 1);

			if (Regex.IsMatch(lastLine, @"^\w:\\[^\x00-\x1F]*>$"))
			{
				Dir = lastLine.Substring(0, lastLine.Length - 1);
			}
		}
	}
}