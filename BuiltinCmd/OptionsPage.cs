using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using CmdHost;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.OLE.Interop;

namespace BuiltinCmd
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class OptionsPage : DialogPage
	{
		[Category("General")]
		[Description("Use PowerShell instead of CMD, restart shell to take effect.")]
		[DisplayName("Use PowerShell instead of CMD")]
		public bool UsePs { get; set; } = false;

		[Category("General")]
		[Description("Terminal Font to Use, restart shell to take effect.")]
		[DisplayName("Terminal Font")]
		public string Font { get; set; } = "Consolas";

		[Category("General")]
		[Description("Terminal Font Size, restart shell to take effect.")]
		[DisplayName("Terminal Font Size")]
		public int FontSize { get; set; } = 10;

		[Category("Parameter")]
		[Description("Global Startup Commands, execute when VS started.")]
		[DisplayName("Global Startup Commands")]
		public string InitScript { get; set; } = "";

		[Category("Parameter")]
		[Description("Project-wide Startup Commands, execute when project opened.")]
		[DisplayName("Project Startup Commands")]
		public string ProjectInitScript { get; set; } = "";
	}
}