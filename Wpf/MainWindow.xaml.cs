using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CmdHost;
using Microsoft.Win32;

namespace Wpf
{
	public partial class MainWindow : Window, ITerminalBoxProvider
	{
		private readonly TerminalController controller;

		public MainWindow()
		{
			InitializeComponent();

			Rst.BorderThickness = new Thickness(0, 0, 0, 0);

			controller = new TerminalController(this);
		}

		private void OnLoad(object sender, EventArgs e)
		{
			controller.Init();

			Rst.Focus();

			Closing += (s, ev) =>
			{
				controller.Close();
			};
		}

		public TextBox GetTextBox()
		{
			return this.Rst;
		}

		private void OnClear(object sender, EventArgs e)
		{
			controller.ClearOutput();
		}
		private void OnRestart(object sender, EventArgs e)
		{
			controller.RestartProc();
		}

		private void OnSave(object sender, EventArgs e)
		{
			SaveFileDialog saveDlg = new SaveFileDialog
			{
				Filter = "txt文件|*.txt|所有文件|*.*",
				FilterIndex = 2,
				RestoreDirectory = true,
				DefaultExt = ".txt",
				AddExtension = true,
				Title = "Save Cmd Results"
			};

			if (saveDlg.ShowDialog() == true)
			{
				FileStream saveStream = new FileStream(saveDlg.FileName, FileMode.Create);
				byte[] data = new UTF8Encoding().GetBytes(Rst.Text);
				saveStream.Write(data, 0, data.Length);
				saveStream.Flush();
				saveStream.Close();
			}
		}

		private void OnCopy(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(Rst.SelectedText);
		}

		private void OnPaste(object sender, RoutedEventArgs e)
		{
			//var text = Clipboard.GetText();

			Rst.Paste();
		}
	}
}
