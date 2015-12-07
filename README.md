F5BIGIP.NET
===========

Making F5's iControl API much easier to work with

F5's [iControl API](https://devcentral.f5.com/icontrol) is powerful but awkward to use. Usage results in code that looks like this:

    var poolName = F5Interfaces.LocalLBPool.get_list().FirstOrDefault(x => x == name);
    var description = F5Interfaces.LocalLBPool.get_description(new[] { poolName }).FirstOrDefault();
    var loadBalancingMethod = F5Interfaces.LocalLBPool.get_lb_method(new[] { poolName }).FirstOrDefault();
    var members = F5Interfaces.LocalLBPool.get_member(new[] { poolName }).FirstOrDefault();
    var monitorAssociations = F5Interfaces.LocalLBPool.get_monitor_association(new[] { poolName }).FirstOrDefault();

The F5 library in this solution contains a more usable object oriented API that lets you write code like this:

    private static void ListVirtualServers(string bigIp)
    {
        Console.WriteLine("Listing virtual servers on: {0}", bigIp);
        var virtualServers = Context.FindAllVirtualServers();
        foreach (var virtualServer in virtualServers)
        {
            Console.WriteLine("Name: {0}, Description: {1}", virtualServer.Name, virtualServer.Description);
        }
    }

****************************************************************

f5ltm.exe
=========

There is a console app, f5ltm.exe, that exposes a lot of this functionality for easy scriptability:

    f5tlm bigip  [/?]  /user  [/password]  [/listnodes]  [/dumpnode]  [/applynode]  [/deletenode]  [/listpools]  [/dumppool]  [/applypool]  [/deletepool]
          [/listvirtualservers]  [/dumpvirtualserver]  [/applyvirtualserver]  [/deletevirtualserver]  [/listrules]  [/dumprule]  [/listmonitors]

    bigip                   The address of the BigIP to manage.
    [/?]                    Show help
    [/applynode]            Apply a node from a file containing a json definition /applynode:myNode.json
    [/applypool]            Apply a pool from a file containing a json definition /applypool:myPool.json
    [/applyvirtualserver]   Apply a virtual server from a file containing a json definition /applyvirtualserver:myVirtualServer.json
    [/deletenode]           Delete a node by its address /deletenode:172.25.12.34
    [/deletepool]           Delete a pool by its name /deletepool:/Common/SomePool
    [/deletevirtualserver]  Delete a virtual server by its name /deletevirtualserver:/Common/SomeVirtualServer
    [/dumpnode]             Dump a node as json to stdout /dumpnode:{name}
    [/dumppool]             Dump a pool as json to stdout /dumppool:{name}
    [/dumprule]             Dump an iRule definition as TCL code to stdout /dumprule:{name}
    [/dumpvirtualserver]    Dump a virtual server as json to stdout /dumpvirtualserver:{name}
    [/listmonitors]         List the monitors
    [/listnodes]            List the nodes
    [/listpools]            List the pools
    [/listrules]            List the iRules
    [/listvirtualservers]   List the virtual servers
    [/password]             Password for BigIP connection. Omit to be prompted.
    /user                   Username for BigIP connection.

The idea is that configuration can be stored in json files, put in version control, and then applied back to the BIGIP LTM with the f5ltm.exe program. There are json templates in the f5ltm project but they look like this:

****************************************************************

JSON Formats
============

node.json
---------
    {
      "Name": "/Common/myNodeName",
      "Address": "10.10.1.2",
      "ConnectionLimit": 0
    }

pool.json
---------
    {
      "Name": "/Common/pool_example",
      "LoadBalancingMethod": "RoundRobin",
      "Members": [
        {
          "Address": "10.10.1.2",
          "Port": 0
        }
      ],
      "Monitors": [
        "/Common/gateway_icmp"
      ]
    }

virtualServer.json
------------------
    {
      "Name": "/Common/vs_my_virtual_server",
      "Address": "10.10.1.3",
      "Port": 80,
      "VirtualServerProtocol": "TransmissionControlProtocol",
      "DefaultPoolName": "/Common/pool_example",
      "Profiles": [
        {
          "VirtualServerProfileContext": "All",
          "Name": "/Common/http"
        },
        {
          "VirtualServerProfileContext": "All",
          "Name": "/Common/tcp"
        }
      ],
      "Vlans": [
        "/Common/MyVlan"
      ],
      "SnatType": "None"
    }

****************************************************************

Using f5ltm.exe
===============

You can list the available nodes/pools/virtual servers by using commands similar to:

    C:\> f5ltm.exe myBigIp /user:myUsername /listnodes
    C:\> f5ltm.exe myBigIp /user:myUsername /listpools
    C:\> f5ltm.exe myBigIp /user:myUsername /listvirtualservers

You can dump the json for an existing node/pool/virtual server by using commands similar to:

    C:\> f5ltm.exe myBigIp /user:myUsername /dumpnode:/Common/MyNode
    C:\> f5ltm.exe myBigIp /user:myUsername /dumppool:/Common/MyPool
    C:\> f5ltm.exe myBigIp /user:myUsername /dumpvirtualserver:/Common/MyVirtualServer

You can apply a json file back to the server by using commands similar to:

    C:\> f5ltm.exe myBigIp /user:myUsername /applynode:myNode.json
    C:\> f5ltm.exe myBigIp /user:myUsername /applypool:myPool.json
    C:\> f5ltm.exe myBigIp /user:myUsername /applyvirtualserver:myVirtualServer.json

The functionality is limited at the moment but I'm adding more things as I need them.

****************************************************************

Known Issues
============
* Setting multiple monitors on pools only applies one.
