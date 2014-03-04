using System;
using System.Text;
using CmdLine;

namespace f5ltm
{
	[CommandLineArguments(Program = "f5tlm", Title = "f5ltm", Description = "Manage the F5 BIG IP LTM")]
	internal class Arguments
	{
		private string _password;

		[CommandLineParameter(Command = "?", Default = false, Description = "Show help", Name = "Help", IsHelp = true)]
		public bool Help { get; set; }

		[CommandLineParameter(Name = "bigip", ParameterIndex = 1, Required = true,
			Description = "The address of the BigIP to manage.")]
		public string BigIp { get; set; }

		[CommandLineParameter(Command = "user", Description = "Username for BigIP connection.", Required = true)]
		public string UserName { get; set; }

		[CommandLineParameter(Command = "password", Description = "Password for BigIP connection. Omit to be prompted.", Required = false)]
		public string Password
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_password))
					_password = ReadPasswordFromConsole();
				return _password;
			}
			set { _password = value; }
		}

		[CommandLineParameter(Command = "listnodes", Description = "List the nodes", Required = false)]
		public bool ListNodes { get; set; }

		[CommandLineParameter(Command = "dumpnode", Description = "Dump a node as json to stdout by its address /dumpnode:10.10.2.5", Required = false)]
		public string DumpNodeAddress { get; set; }

		[CommandLineParameter(Command = "applynode", Description = "Apply a node from a file containing a json definition /applynode:myNode.json", Required = false)]
		public string ApplyNodeFile { get; set; }

		[CommandLineParameter(Command = "deletenode", Description = "Delete a node by its address /deletenode:10.10.2.5", Required = false)]
		public string DeleteNodeAddress { get; set; }

		[CommandLineParameter(Command = "listpools", Description = "List the pools", Required = false)]
		public bool ListPools { get; set; }

		[CommandLineParameter(Command = "dumppool", Description = "Dump a pool as json to stdout /dumppool:{name}", Required = false)]
		public string DumpPoolName { get; set; }

		[CommandLineParameter(Command = "applypool", Description = "Apply a pool from a file containing a json definition /applypool:myPool.json", Required = false)]
		public string ApplyPoolFile { get; set; }

		[CommandLineParameter(Command = "deletepool", Description = "Delete a pool by its name /deletepool:/Common/SomePool", Required = false)]
		public string DeletePoolName { get; set; }

		[CommandLineParameter(Command = "listvirtualservers", Description = "List the virtual servers", Required = false)]
		public bool ListVirtualServers { get; set; }

		[CommandLineParameter(Command = "dumpvirtualserver", Description = "Dump a virtual server as json to stdout /dumpvirtualserver:{name}", Required = false)]
		public string DumpVirtualServerName { get; set; }

		[CommandLineParameter(Command = "applyvirtualserver", Description = "Apply a virtual server from a file containing a json definition /applyvirtualserver:myVirtualServer.json", Required = false)]
		public string ApplyVirtualServerFile { get; set; }

		[CommandLineParameter(Command = "deletevirtualserver", Description = "Delete a virtual server by its name /deletevirtualserver:/Common/SomeVirtualServer", Required = false)]
		public string DeleteVirtualServerName { get; set; }

		[CommandLineParameter(Command = "listrules", Description = "List the iRules", Required = false)]
		public bool ListRules { get; set; }

		[CommandLineParameter(Command = "dumprule", Description = "Dump an iRule definition as TCL code to stdout /dumprule:{name}", Required = false)]
		public string DumpRuleName { get; set; }

		[CommandLineParameter(Command = "listmonitors", Description = "List the monitors", Required = false)]
		public bool ListMonitors { get; set; }

		private string ReadPasswordFromConsole()
		{
			Console.Write("Enter password for {0} on {1}: ", UserName, BigIp);
			var password = new StringBuilder();
			while (true)
			{
				var readKey = Console.ReadKey(true);
				if (readKey.Key == ConsoleKey.Enter)
				{
					Console.Write("\n");
					break;
				}
				if (readKey.Key == ConsoleKey.Backspace)
				{
					if (password.Length > 0)
					{
						password.Remove(password.Length - 1, 1);
						Console.Write("\b \b");
					}
				}
				else
				{
					password.Append(readKey.KeyChar);
					Console.Write("*");
				}
			}
			return password.ToString();
		}
	}
}
