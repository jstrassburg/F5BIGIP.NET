using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using iControl;

namespace F5
{
	public static class Context
	{
		private static readonly Interfaces F5Interfaces = new Interfaces();
		private static Version _version;
		private const short MajorVersionThatSwitchedToV2Classes = 11;

		public static void Initialize(string bigIp, string userName, string password)
		{
			var initialized = F5Interfaces.initialize(bigIp, userName, password);
			if (!initialized)
				throw new F5Exception(F5Interfaces.LastException);
			_version = ParseVersion(F5Interfaces.GlobalLBApplication.get_version());
		}

		public static void Initialize(string bigIp, long port, string userName, string password)
		{
			var initialized = F5Interfaces.initialize(bigIp, port, userName, password);
			if (!initialized)
				throw new F5Exception(F5Interfaces.LastException);
			_version = ParseVersion(F5Interfaces.GlobalLBApplication.get_version());
		}

		/// <summary>
		/// Parse the <see cref="Version"/> out of the string that comes back from iControl's
		/// GlobalLBApplication.get_version() which looks like: "BIG-IP _v11.2.0"
		/// </summary>
		/// <param name="bigIpVersion">The result of GlobalLBApplication.get_version()</param>
		/// <returns>A parsed <see cref="Version"/> instance.</returns>
		private static Version ParseVersion(string bigIpVersion)
		{
			var match = Regex.Match(bigIpVersion, @"\d*\.\d*\.\d*");
			return new Version(match.ToString());
		}

		public static Node FindNode(string address)
		{
			var nodeAddress = F5Interfaces.LocalLBNodeAddress.get_list().FirstOrDefault(x => x == address);
			if (string.IsNullOrEmpty(nodeAddress))
				return null;

			var name = F5Interfaces.LocalLBNodeAddress.get_screen_name(new[] { nodeAddress }).FirstOrDefault() ?? string.Empty;
			var connectionLimit = F5Interfaces.LocalLBNodeAddress.get_connection_limit(new[] { nodeAddress }).FirstOrDefault();

			return new Node { Name = name, Address = nodeAddress, ConnectionLimit = connectionLimit == null ? 0 : connectionLimit.low };
		}

		public static IEnumerable<Node> FindAllNodes()
		{
			var nodeAddresses = F5Interfaces.LocalLBNodeAddress.get_list();
			var nodes = nodeAddresses.Select(FindNode);

			return nodes;
		}

		public static void ApplyNode(Node node)
		{
			var existingNode = FindNode(node.Address);
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
			F5Interfaces.LocalLBNodeAddress.set_connection_limit(
				new[] { node.Address },
				new[] { new CommonULong64 { low = node.ConnectionLimit } });
		}

		private static void InsertNewNode(Node node)
		{
			if (_version.Major >= MajorVersionThatSwitchedToV2Classes)
			{
				F5Interfaces.LocalLBNodeAddressV2.create(
					new[] { node.Name },
					new[] { node.Address },
					new[] { node.ConnectionLimit });
			}
			else
			{
				F5Interfaces.LocalLBNodeAddress.create(
				new[] { node.Address },
				new[] { node.ConnectionLimit });
				F5Interfaces.LocalLBNodeAddress.set_screen_name(
					new[] { node.Address },
					new[] { node.Name });
			}
		}

		public static void DeleteNode(string address)
		{
			F5Interfaces.LocalLBNodeAddress.delete_node_address(new[] { address });
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
			var loadBalancingMethod = F5Interfaces.LocalLBPool.get_lb_method(new[] { poolName }).FirstOrDefault();
			var members = F5Interfaces.LocalLBPool.get_member(new[] { poolName }).FirstOrDefault();
			var monitorAssociations = F5Interfaces.LocalLBPool.get_monitor_association(new[] { poolName }).FirstOrDefault();

			var pool = new Pool
			{
				Name = poolName,
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

			var destination = F5Interfaces.LocalLBVirtualServer.get_destination(new[] { name }).First();
			var protocol = F5Interfaces.LocalLBVirtualServer.get_protocol(new[] { name }).First();
			var defaultPoolName = F5Interfaces.LocalLBVirtualServer.get_default_pool_name(new[] { name }).First();
			var profile = F5Interfaces.LocalLBVirtualServer.get_profile(new[] { name }).First();
			var vlans = F5Interfaces.LocalLBVirtualServer.get_vlan(new[] { name }).First();
			var snatType = F5Interfaces.LocalLBVirtualServer.get_snat_type(new[] { name }).First();

			var virtualServer = new VirtualServer
			{
				Name = virtualServerName,
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
				Vlans = vlans.vlans,
				SnatType = (SnatType)snatType,
			};
			return virtualServer;
		}

		public static void ApplyVirtualServer(VirtualServer virtualServer)
		{
			if (virtualServer.SnatType != SnatType.None && virtualServer.SnatType != SnatType.Automap)
			{
				throw new F5Exception(string.Format("Unsupported SnatType: {0}", virtualServer.SnatType));
			}

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

			F5Interfaces.LocalLBVirtualServer.set_vlan(
				new[] { virtualServer.Name },
				new[] { new CommonVLANFilterList
				{
					state = CommonEnabledState.STATE_ENABLED,
					vlans = virtualServer.Vlans.ToArray()
				}});

			if (virtualServer.SnatType == SnatType.Automap)
			{
				F5Interfaces.LocalLBVirtualServer.set_snat_automap(new[] { virtualServer.Name });
			}
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
