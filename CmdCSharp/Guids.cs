// Guids.cs
// MUST match guids.h
using System;

namespace Lx.CmdCSharp
{
	static class GuidList
	{
		public const string guidCmdCSharpPkgString = "5f1a1460-a418-4fae-9cad-ff1c76b7aed9";
		public const string guidCmdCSharpCmdSetString = "1bcda133-5334-485a-a274-ab679515e8f3";
		public const string guidToolWindowPersistanceString = "04680063-ebbc-434a-a89e-9ebead640025";

		public static readonly Guid guidCmdCSharpCmdSet = new Guid(guidCmdCSharpCmdSetString);
	};
}