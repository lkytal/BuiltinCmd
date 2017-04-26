using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace Wpf
{
	public partial class MainWindow : Window
	{
		private readonly Controller controller;

		public MainWindow()
		{
			InitializeComponent();

			Rst.BorderThickness = new Thickness(0, 0, 0, 0);

			controller = new Controller(this);
		}

		private void OnLoad(object sender, EventArgs e)
		{
			controller.cmdReader.Init();

			Rst.Focus();

			Closing += (s, ev) =>
			{
				controller.Close();
			};
		}

		private void OnText(object sender, KeyEventArgs e)
		{
			controller.HandleInput(e);
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
	}
}
