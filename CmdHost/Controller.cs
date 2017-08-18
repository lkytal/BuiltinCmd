using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CmdHost
{
	public interface UI
	{
		Dispatcher Dispatcher { get; }

		TextBox GetTextBox();
	}

	public class Controller : CmdReceiver, TextBoxSource
	{
		private readonly UI mainWindow;
		private readonly HistoryCommand historyCommand;
		private readonly CmdReader cmdReader;
		private readonly TabHandler tabHandler;
		private readonly Terminal terminal;

		public Controller(UI _ui)
		{
			mainWindow = _ui;

			mainWindow.GetTextBox().PreviewKeyDown += (sender, e) =>
			{
				HandleInput(e);
			};

			historyCommand = new HistoryCommand();
			cmdReader = new CmdReader();
			cmdReader.Register(this);

			terminal = new Terminal(this);
			tabHandler = new TabHandler(terminal);
		}

		public void Init()
		{
			cmdReader.Init();
		}

		public void Init(string projectPath)
		{
			cmdReader.Init(projectPath);
		}

		public void AddData(string outputs)
		{
			tabHandler.ExtractDir(outputs);
			tabHandler.ResetTabComplete();

			mainWindow.Dispatcher.Invoke(() =>
			{
				terminal.AppendOutput(outputs);
			});
		}

		public void InvokeCmd(string msg, string cmd)
		{
			mainWindow.Dispatcher.Invoke(() =>
			{
				terminal.AppendOutput(msg);
			});

			cmdReader.Input(cmd);
		}

		private void RunCmd()
		{
			string cmd = terminal.GetCmd();

			if (cmd == "cls")
			{
				mainWindow.Dispatcher.Invoke(() =>
				{
					terminal.Clear();
				});

				cmdReader.Input("");
			}
			else
			{
				//No async, ensure is done
				mainWindow.Dispatcher.Invoke(() =>
				{
					terminal.RemoveInput();
				});

				cmdReader.Input(cmd);
			}

			historyCommand.Add(cmd);
		}

		public void HandleInput(KeyEventArgs e)
		{
			if (e.Key == Key.C && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
			{
				cmdReader.SendCtrlC();
				e.Handled = true;
				return;
			}

			if (NoEditArea(e))
			{
				if (IsCharactor(e.Key))
				{
					terminal.FocusEnd();
				}
				else if (e.Key >= Key.Left && e.Key <= Key.Down)
				{
					return;
				}
				else
				{
					e.Handled = true;
					return;
				}
			}

			if (e.Key == Key.Tab)
			{
				tabHandler.HandleTab();
				e.Handled = true;
				return;
			}
			else
			{
				tabHandler.ResetTabComplete();
			}

			if (IsntControlKeys(e))
			{
				e.Handled = true;
			}
		}

		private bool IsCharactor(Key key)
		{
			if (key < Key.Z && key > Key.D0)
			{
				return true;
			}

			return false;
		}

		private bool IsntControlKeys(KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Up:
					terminal.SetInput(historyCommand.SelectPreviuos());
					break;

				case Key.Down:
					terminal.SetInput(historyCommand.SelectNext());
					break;

				case Key.Home:
					terminal.FocusEnd();
					break;

				case Key.Return:
					RunCmd();
					break;

				default:
					return false;
			}

			return true;
		}

		private bool NoEditArea(KeyEventArgs e)
		{
			if (terminal.CaretIndex < terminal.DataLen)
			{
				return true;
			}

			if (e.Key == Key.Back && terminal.CaretIndex <= terminal.DataLen)
			{
				return true;
			}

			return false;
		}

		public void Close()
		{
			//cmdReader.Close(); Needless
		}

		public void ClearOutput()
		{
			terminal.Clear();
			cmdReader.Input("");
		}

		public void RestartProc()
		{
			terminal.Clear();
			cmdReader.Restart();
		}

		public TextBox GetTextBox()
		{
			return mainWindow.GetTextBox();
		}
	}
}