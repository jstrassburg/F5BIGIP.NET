using System;
using System.IO;
using CmdLine;
using F5;
using Newtonsoft.Json;

namespace f5ltm
{
	internal sealed class Program
	{
		static void Main()
		{
			try
			{
				var arguments = CommandLine.Parse<Arguments>();
				Context.Initialize(arguments.BigIp, arguments.UserName, arguments.Password);

				if (arguments.ListNodes)
				{
					ListNodes(arguments.BigIp);
				}
				if (!string.IsNullOrEmpty(arguments.DumpNodeName))
				{
					DumpNode(arguments.DumpNodeName, arguments.BigIp);
				}
				if (!string.IsNullOrEmpty(arguments.ApplyNodeFile))
				{
					ApplyNode(arguments.ApplyNodeFile, arguments.BigIp);
				}
				if (!string.IsNullOrEmpty(arguments.DeleteNodeName))
				{
					DeleteNode(arguments.DeleteNodeName, arguments.BigIp);
				}

				if (arguments.ListPools)
				{
					ListPools(arguments.BigIp);
				}
				if (!string.IsNullOrEmpty(arguments.DumpPoolName))
				{
					DumpPool(arguments.DumpPoolName, arguments.BigIp);
				}
				if (!string.IsNullOrEmpty(arguments.ApplyPoolFile))
				{
					ApplyPool(arguments.ApplyPoolFile, arguments.BigIp);
				}
				if (!string.IsNullOrEmpty(arguments.DeletePoolName))
				{
					DeletePool(arguments.DeletePoolName, arguments.BigIp);
				}

				if (arguments.ListVirtualServers)
				{
					ListVirtualServers(arguments.BigIp);
				}
				if (!string.IsNullOrEmpty(arguments.DumpVirtualServerName))
				{
					DumpVirtualServer(arguments.DumpVirtualServerName, arguments.BigIp);
				}
				if (!string.IsNullOrEmpty(arguments.ApplyVirtualServerFile))
				{
					ApplyVirtualServer(arguments.ApplyVirtualServerFile, arguments.BigIp);
				}
				if (!string.IsNullOrEmpty(arguments.DeleteVirtualServerName))
				{
					DeleteVirtualServer(arguments.DeleteVirtualServerName, arguments.BigIp);
				}

				if (arguments.ListRules)
				{
					ListRules(arguments.BigIp);
				}
				if (!string.IsNullOrEmpty(arguments.DumpRuleName))
				{
					DumpRule(arguments.DumpRuleName, arguments.BigIp);
				}

				if (arguments.ListMonitors)
				{
					ListMonitors(arguments.BigIp);
				}
			}
			catch (CommandLineException exception)
			{
				Console.WriteLine(exception.ArgumentHelp.GetHelpText(Console.BufferWidth));
			}
			catch (F5Exception exception)
			{
				Console.WriteLine(exception);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
			}
		}

		private static void ListMonitors(string bigIp)
		{
			Console.WriteLine("Listing monitors on: {0}", bigIp);
			var monitors = Context.FindAllMonitors();
			foreach (var monitor in monitors)
			{
				Console.WriteLine("Name: {0}, Type: {1}", monitor.Name, monitor.MonitorType);
			}
		}

		private static void DeleteVirtualServer(string deleteVirtualServerName, string bigIp)
		{
			if (Context.FindVirtualServer(deleteVirtualServerName) == null)
				Console.WriteLine("Virtual server named {0} was not found on {1}.", deleteVirtualServerName, bigIp);
			else
			{
				Console.WriteLine("Deleting virtual server with name: {0} from {1}", deleteVirtualServerName, bigIp);
				Context.DeleteVirtualServer(deleteVirtualServerName);
			}
		}

		private static void ApplyVirtualServer(string applyVirtualServerFile, string bigIp)
		{
			var jsonVirtualServer = File.ReadAllText(applyVirtualServerFile);

			Console.WriteLine("Applying the following virtual server to {0}\n{1}", bigIp, jsonVirtualServer);

			var virtualServer = JsonConvert.DeserializeObject<VirtualServer>(jsonVirtualServer);
			Context.ApplyVirtualServer(virtualServer);
		}

		private static void DumpVirtualServer(string dumpVirtualServerName, string bigIp)
		{
			var virtualServer = Context.FindVirtualServer(dumpVirtualServerName);
			if (virtualServer == null)
				Console.WriteLine("Virtual server: {0} not found on {1}.", dumpVirtualServerName, bigIp);
			else
			{
				Console.WriteLine(JsonConvert.SerializeObject(virtualServer, Formatting.Indented));
			}
		}

		private static void ListVirtualServers(string bigIp)
		{
			Console.WriteLine("Listing virtual servers on: {0}", bigIp);
			var virtualServers = Context.FindAllVirtualServers();
			foreach (var virtualServer in virtualServers)
			{
				Console.WriteLine("Name: {0}", virtualServer.Name);
			}
		}

		private static void DeletePool(string deletePoolName, string bigIp)
		{
			if (Context.FindPool(deletePoolName) == null)
				Console.WriteLine("Pool named {0} was not found on {1}.", deletePoolName, bigIp);
			else
			{
				Console.WriteLine("Deleting pool with name: {0} from {1}", deletePoolName, bigIp);
				Context.DeletePool(deletePoolName);
			}
		}

		private static void DumpRule(string dumpRuleName, string bigIp)
		{
			var rule = Context.FindRule(dumpRuleName);
			if (rule == null)
				Console.WriteLine("iRule: {0} not found on {1}.", dumpRuleName, bigIp);
			else
			{
				Console.WriteLine(rule.Code);
			}
		}

		private static void ListRules(string bigIp)
		{
			Console.WriteLine("Listing iRules on: {0}", bigIp);
			var rules = Context.FindAllRules();
			foreach (var rule in rules)
			{
				Console.WriteLine("Name: {0}", rule.Name);
			}
		}

		private static void ListPools(string bigIp)
		{
			Console.WriteLine("Listing pools on: {0}", bigIp);
			var pools = Context.FindAllPools();
			foreach (var pool in pools)
			{
				Console.WriteLine("Name: {0}, Load balancing method: {1}", pool.Name, pool.LoadBalancingMethod);
				foreach (var member in pool.Members)
				{
					Console.WriteLine("\tMember - Address: {0}, Port: {1}", member.Address, member.Port);
				}
			}
		}

		private static void DumpPool(string dumpPoolName, string bigIp)
		{
			var pool = Context.FindPool(dumpPoolName);
			if (pool == null)
				Console.WriteLine("Pool: {0}, not found on {1}.", dumpPoolName, bigIp);
			else
			{
				Console.WriteLine(JsonConvert.SerializeObject(pool, Formatting.Indented));
			}
		}

		private static void DeleteNode(string nodeName, string bigIp)
		{
			Console.WriteLine("Deleting node with name: {0} from {1}", nodeName, bigIp);
			Context.DeleteNode(nodeName);
		}

		private static void ApplyNode(string applyNodeFile, string bigIp)
		{
			var jsonNode = File.ReadAllText(applyNodeFile);

			Console.WriteLine("Applying the following node to {0}\n{1}", bigIp, jsonNode);

			var node = JsonConvert.DeserializeObject<Node>(jsonNode);
			Context.ApplyNode(node);
		}

		private static void ApplyPool(string applyPoolFile, string bigIp)
		{
			var jsonPool = File.ReadAllText(applyPoolFile);

			Console.WriteLine("Applying the following pool to {0}\n{1}", bigIp, jsonPool);

			var pool = JsonConvert.DeserializeObject<Pool>(jsonPool);
			Context.ApplyPool(pool);
		}

		private static void DumpNode(string dumpNodeName, string bigIp)
		{
			var node = Context.FindNode(dumpNodeName);
			if (node == null)
				Console.WriteLine("Node: {0}, not found on {1}.", dumpNodeName, bigIp);
			else
			{
				Console.WriteLine(JsonConvert.SerializeObject(node, Formatting.Indented));
			}
		}

		private static void ListNodes(string bigIp)
		{
			Console.WriteLine("Listing nodes on: {0}", bigIp);
			var nodes = Context.FindAllNodes();
			foreach (var node in nodes)
			{
				Console.WriteLine("Name: {0}, Address: {1}, Connection limit: {2}, Description: {3}",
					node.Name, node.Address, node.ConnectionLimit, node.Description);
			}
		}
	}
}
