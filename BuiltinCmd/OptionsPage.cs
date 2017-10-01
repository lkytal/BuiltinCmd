using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using CmdHost;

namespace BuiltinCmd
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class OptionsPage : DialogPage
	{
		[Category("General")]
		[Description("Use PowerShell instead of CMD, restart shell to take effect.")]
		[DisplayName("Use PowerShell instead of CMD")]
		public bool usePS { get; set; } = false;

		[Category("Parameter")]
		[Description("Global Startup Commands, execute when VS started.")]
		[DisplayName("Global Startup Commands")]
		public string initScript { get; set; } = "";

		[Category("Parameter")]
		[Description("Project-wide Startup Commands, execute when project opened.")]
		[DisplayName("Project Startup Commands")]
		public string projectInitScript { get; set; } = "";
	}
}