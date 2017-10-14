using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CmdHost
{
	public interface ITerminalBoxProvider
	{
		Dispatcher Dispatcher { get; }

		TextBox GetTextBox();
	}

	public class TerminalController : ICmdReceiver, ITextBoxSource
	{
		private readonly ITerminalBoxProvider mainWindow;
		private readonly HistoryCommand historyCommand;
		private readonly CmdReader cmdReader;
		private readonly TabHandler tabHandler;
		private readonly TerminalContentMgr terminalContentMgr;

		public TerminalController(ITerminalBoxProvider terminalBoxProvider)
		{
			mainWindow = terminalBoxProvider;

			mainWindow.GetTextBox().PreviewKeyDown += (sender, e) =>
			{
				HandleInput(e);
			};

			historyCommand = new HistoryCommand();
			cmdReader = new CmdReader();
			cmdReader.Register(this);

			terminalContentMgr = new TerminalContentMgr(this);
			tabHandler = new TabHandler(terminalContentMgr);
		}

		public void Init(string projectPath = null)
		{
			cmdReader.InitDir = projectPath;
			cmdReader.Init();
		}

		public void SetPath(string projectPath)
		{
			cmdReader.InitDir = projectPath;
		}

		public void SetShell(string shell)
		{
			cmdReader.Shell = shell;
		}

		public void AddData(string outputs)
		{
			tabHandler.ExtractDir(outputs);
			tabHandler.ResetTabComplete();

			mainWindow.Dispatcher.Invoke(() =>
			{
				terminalContentMgr.AppendOutput(outputs);
			});
		}

		public void InvokeCmd(string msg, string cmd = "")
		{
			if (String.IsNullOrEmpty(cmd))
			{
				return;
			}

			mainWindow.Dispatcher.Invoke(() =>
			{
				terminalContentMgr.AppendOutput(msg);
			});

			cmdReader.Input(cmd);
		}

		private void RunCmd()
		{
			string cmd = terminalContentMgr.GetCmd();

			if (cmd == "cls")
			{
				mainWindow.Dispatcher.Invoke(() =>
				{
					terminalContentMgr.Clear();
				});

				cmdReader.Input("");
			}
			else
			{
				//No async, ensure is done
				mainWindow.Dispatcher.Invoke(() =>
				{
					terminalContentMgr.RemoveInput();
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
				if (IsCharactorOrEnter(e.Key))
				{
					terminalContentMgr.FocusEnd();
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

			if (IsControlKeys(e))
			{
				e.Handled = true;
			}
		}

		private bool IsCharactorOrEnter(Key key)
		{
			if (key < Key.Z && key > Key.D0)
			{
				return true;
			}
			else if (key == Key.Enter)
			{
				return true;
			}

			return false;
		}

		private bool IsControlKeys(KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Up:
					terminalContentMgr.SetInput(historyCommand.SelectPreviuos());
					break;

				case Key.Down:
					terminalContentMgr.SetInput(historyCommand.SelectNext());
					break;

				case Key.Home:
					terminalContentMgr.FocusEnd();
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
			if (terminalContentMgr.CaretIndex < terminalContentMgr.DataLen)
			{
				return true;
			}

			if (e.Key == Key.Back && terminalContentMgr.CaretIndex <= terminalContentMgr.DataLen)
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
			terminalContentMgr.Clear();
			cmdReader.Input("");
		}

		public void RestartProc()
		{
			terminalContentMgr.Clear();
			cmdReader.Restart();
		}

		public TextBox GetTextBox()
		{
			return mainWindow.GetTextBox();
		}
	}
}