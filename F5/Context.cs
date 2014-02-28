using System.Collections.Generic;
using System.Linq;
using iControl;

namespace F5
{
	public static class Context
	{
		internal static readonly Interfaces F5Interfaces = new Interfaces();

		public static void Initialize(string bigIp, string userName, string password)
		{
			var initialized = F5Interfaces.initialize(bigIp, userName, password);
			if (!initialized)
				throw new F5Exception(F5Interfaces.LastException);
		}

		public static void Initialize(string bigIp, long port, string userName, string password)
		{
			var initialized = F5Interfaces.initialize(bigIp, port, userName, password);
			if (!initialized)
				throw new F5Exception(F5Interfaces.LastException);
		}

		public static Node FindNode(string name)
		{
			var nodeName = F5Interfaces.LocalLBNodeAddressV2.get_list().FirstOrDefault(x => x == name);
			if (nodeName == null)
				return null;
			var address = F5Interfaces.LocalLBNodeAddressV2.get_address(new[] { nodeName }).FirstOrDefault();
			var connectionLimit = F5Interfaces.LocalLBNodeAddressV2.get_connection_limit(new[] { nodeName }).FirstOrDefault();
			var description = F5Interfaces.LocalLBNodeAddressV2.get_description(new[] { nodeName }).FirstOrDefault();
			return new Node { Name = nodeName, Address = address, ConnectionLimit = connectionLimit, Description = description };
		}

		public static IEnumerable<Node> FindAllNodes()
		{
			var nodeNames = F5Interfaces.LocalLBNodeAddressV2.get_list();
			var nodes = nodeNames.Select(FindNode);

			return nodes;
		}

		public static void ApplyNode(Node node)
		{
			var existingNode = FindNode(node.Name);
			if (existingNode == null)
			{
				InsertNewNode(node);
			}
			else
			{
				UpdateNode(node);
			}
		}

		private static void UpdateNode(Node node)
		{
			// address and name appear to be immutable
			F5Interfaces.LocalLBNodeAddressV2.set_connection_limit(
				new[] { node.Name },
				new[] { node.ConnectionLimit ?? 0 });
			F5Interfaces.LocalLBNodeAddressV2.set_description(
				new[] { node.Name },
				new[] { node.Description });
		}

		private static void InsertNewNode(Node node)
		{
			F5Interfaces.LocalLBNodeAddressV2.create(
				new[] { node.Name },
				new[] { node.Address },
				new[] { node.ConnectionLimit ?? 0 });
			if (!string.IsNullOrWhiteSpace(node.Description))
				F5Interfaces.LocalLBNodeAddressV2.set_description(
					new[] { node.Name },
					new[] { node.Description });
		}

		public static void DeleteNode(string nodeName)
		{
			F5Interfaces.LocalLBNodeAddressV2.delete_node_address(new[] { nodeName });
		}

		public static IEnumerable<Rule> FindAllRules()
		{
			var rules = F5Interfaces.LocalLBRule.query_all_rules().OrderBy(x => x.rule_name);
			return rules.Select(x => new Rule { Name = x.rule_name, Code = x.rule_definition.Trim() });
		}

		public static Rule FindRule(string name)
		{
			var rule = F5Interfaces.LocalLBRule.query_all_rules().FirstOrDefault(x => x.rule_name == name);
			if (rule == null)
				return null;
			return new Rule { Name = rule.rule_name, Code = rule.rule_definition.Trim() };
		}

		public static IEnumerable<Pool> FindAllPools()
		{
			var poolNames = F5Interfaces.LocalLBPool.get_list();
			var pools = poolNames.Select(FindPool);

			return pools;
		}

		public static Pool FindPool(string name)
		{
			var poolName = F5Interfaces.LocalLBPool.get_list().FirstOrDefault(x => x == name);
			if (poolName == null)
				return null;
			var description = F5Interfaces.LocalLBPool.get_description(new[] { poolName }).FirstOrDefault();
			var loadBalancingMethod = F5Interfaces.LocalLBPool.get_lb_method(new[] { poolName }).FirstOrDefault();
			var members = F5Interfaces.LocalLBPool.get_member(new[] { poolName }).FirstOrDefault();
			var monitorAssociations = F5Interfaces.LocalLBPool.get_monitor_association(new[] { poolName }).FirstOrDefault();

			var pool = new Pool
			{
				Name = poolName,
				Description = description,
				LoadBalancingMethod = (LoadBalancingMethod)loadBalancingMethod,
				Members = members == null
					? new List<PoolMember>()
					: members.Select(x => new PoolMember { Address = x.address, Port = x.port }),
				Monitors = monitorAssociations == null
					? new List<string>()
					: monitorAssociations.monitor_rule.monitor_templates.ToList()
			};

			return pool;
		}

		public static void ApplyPool(Pool pool)
		{
			var existingPool = FindPool(pool.Name);
			if (existingPool == null)
			{
				InsertNewPool(pool);
			}
			else
			{
				UpdatePool(pool);
			}
		}

		private static void UpdatePool(Pool pool)
		{
			F5Interfaces.LocalLBPool.remove_monitor_association(new[] { pool.Name });
			var members = F5Interfaces.LocalLBPool.get_member(new[] { pool.Name }).First().ToArray();
			F5Interfaces.LocalLBPool.remove_member(
				new[] { pool.Name },
				new[] { members });

			F5Interfaces.LocalLBPool.set_lb_method(
				new[] { pool.Name },
				new[] { (LocalLBLBMethod)pool.LoadBalancingMethod });
			F5Interfaces.LocalLBPool.set_description(
				new[] { pool.Name },
				new[] { pool.Description });
			F5Interfaces.LocalLBPool.add_member(
				new[] { pool.Name },
				new[] { pool.Members.Select(x => new CommonIPPortDefinition { address = x.Address, port = x.Port }).ToArray() });
			F5Interfaces.LocalLBPool.set_monitor_association(
				new[] {new LocalLBPoolMonitorAssociation
				{
					pool_name = pool.Name,
					monitor_rule = new LocalLBMonitorRule
					{
						quorum = 0,
						type = LocalLBMonitorRuleType.MONITOR_RULE_TYPE_SINGLE,
						monitor_templates = pool.Monitors.ToArray()
					}
				}});
		}

		private static void InsertNewPool(Pool pool)
		{
			F5Interfaces.LocalLBPool.create(
				new[] { pool.Name },
				new[] { (LocalLBLBMethod)pool.LoadBalancingMethod },
				new[] { pool.Members.Select(x => new CommonIPPortDefinition { address = x.Address, port = x.Port }).ToArray() });
			F5Interfaces.LocalLBPool.set_description(
				new[] { pool.Name },
				new[] { pool.Description });
			F5Interfaces.LocalLBPool.set_monitor_association(
				new[] {new LocalLBPoolMonitorAssociation
				{
					pool_name = pool.Name,
					monitor_rule = new LocalLBMonitorRule
					{
						quorum = 0,
						type = LocalLBMonitorRuleType.MONITOR_RULE_TYPE_SINGLE,
						monitor_templates = pool.Monitors.ToArray()
					}
				}});
		}

		public static void DeletePool(string poolName)
		{
			F5Interfaces.LocalLBPool.delete_pool(new[] { poolName });
		}

		public static IEnumerable<VirtualServer> FindAllVirtualServers()
		{
			var virtualServerNames = F5Interfaces.LocalLBVirtualServer.get_list();
			var virtualServers = virtualServerNames.Select(FindVirtualServer);

			return virtualServers;
		}

		public static VirtualServer FindVirtualServer(string name)
		{
			var virtualServerName = F5Interfaces.LocalLBVirtualServer.get_list().FirstOrDefault(x => x == name);
			if (virtualServerName == null)
				return null;

			var description = F5Interfaces.LocalLBVirtualServer.get_description(new[] { name }).First();
			var destination = F5Interfaces.LocalLBVirtualServer.get_destination(new[] { name }).First();
			var protocol = F5Interfaces.LocalLBVirtualServer.get_protocol(new[] { name }).First();
			var defaultPoolName = F5Interfaces.LocalLBVirtualServer.get_default_pool_name(new[] { name }).First();
			var profile = F5Interfaces.LocalLBVirtualServer.get_profile(new[] { name }).First();

			var virtualServer = new VirtualServer
			{
				Name = virtualServerName,
				Description = description,
				Address = destination.address,
				Port = destination.port,
				VirtualServerProtocol = (VirtualServerProtocol)protocol,
				DefaultPoolName = defaultPoolName,
				Profiles = profile == null
					? new List<VirtualServerProfile>()
					: profile.Select(
									 x => new VirtualServerProfile
									 {
										 Name = x.profile_name,
										 VirtualServerProfileContext = (VirtualServerProfileContext)x.profile_context
									 }),
			};
			return virtualServer;
		}

		public static void ApplyVirtualServer(VirtualServer virtualServer)
		{
			if (FindVirtualServer(virtualServer.Name) != null)
				DeleteVirtualServer(virtualServer.Name);

			F5Interfaces.LocalLBVirtualServer.create(
				new[] { new CommonVirtualServerDefinition
						{
							address = virtualServer.Address, 
							name = virtualServer.Name, 
							port = virtualServer.Port, 
							protocol = (CommonProtocolType)virtualServer.VirtualServerProtocol,
						}
				},
				new[] { "255.255.255.255" },
				new[] { new LocalLBVirtualServerVirtualServerResource
						{
							default_pool_name = virtualServer.DefaultPoolName,
							type = LocalLBVirtualServerVirtualServerType.RESOURCE_TYPE_POOL
						} 
				},
				new[] { virtualServer.Profiles.Select(
					x => new LocalLBVirtualServerVirtualServerProfile
					{
						profile_context = (LocalLBProfileContextType)x.VirtualServerProfileContext,
						profile_name = x.Name
					}).ToArray()}
				);
			F5Interfaces.LocalLBVirtualServer.set_description(
				new[] { virtualServer.Name },
				new[] { virtualServer.Description });
		}

		public static void DeleteVirtualServer(string deleteVirtualServerName)
		{
			F5Interfaces.LocalLBVirtualServer.delete_virtual_server(new[] { deleteVirtualServerName });
		}

		public static IEnumerable<Monitor> FindAllMonitors()
		{
			var monitors =
				F5Interfaces.LocalLBMonitor.get_template_list()
					.Select(x => new Monitor { Name = x.template_name, MonitorType = (MonitorType)x.template_type });

			return monitors;
		}
	}
}
