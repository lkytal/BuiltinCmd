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
		[Description("Use PowerShell instead of CMD.")]
		[DisplayName("Use PowerShell")]
		public bool usePS { get; set; } = false;

		[Category("Parameter")]
		[Description("Global Startup Command, execute when VS started.")]
		[DisplayName("Global Startup Command")]
		public string initScript { get; set; } = "";

		[Category("Parameter")]
		[Description("Project Startup Command, execute when project opened.")]
		[DisplayName("Project Startup Command")]
		public string projectInitScript { get; set; } = "";
	}
}