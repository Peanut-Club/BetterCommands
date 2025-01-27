using System.Collections.Generic;
using System.Linq;
using Compendium;
using PluginAPI.Core.Interfaces;

namespace BetterCommands.Permissions;

public static class PermissionManager
{
	public static PermissionConfig Config => Loader.Config.Permissions;

	public static bool TryGetLevel(ReferenceHub hub, out PermissionLevel permission)
	{
		if (Config.LevelsByPlayer.TryGetValue(hub.UserId(), out permission) || Config.LevelsByPlayer.TryGetValue(hub.connectionToClient.address, out permission))
		{
			return true;
		}
		if (PermissionUtils.TryGetGroupKeys(hub, out var groupKeys))
		{
			string[] array = groupKeys;
			permission = PermissionLevel.None;
			string[] array2 = array;
			foreach (string key in array2)
			{
				if (Config.LevelsByPlayer.TryGetValue(key, out var value) && permission < value)
				{
					permission = value;
				}
			}
			if (permission > PermissionLevel.None)
			{
				return true;
			}
		}
		string clearId;
		return PermissionUtils.TryGetClearId(hub, out clearId) && Config.LevelsByPlayer.TryGetValue(clearId, out permission);
	}

	public static bool TryGetNodes(ReferenceHub hub, out string[] nodes)
	{
		if (Config.NodesByPlayer.TryGetValue(hub.UserId(), out nodes) || Config.NodesByPlayer.TryGetValue(hub.connectionToClient.address, out nodes))
		{
			return true;
		}
		if (PermissionUtils.TryGetGroupKeys(hub, out var groupKeys))
		{
			string[] array = groupKeys;
			string[] array2 = array;
			foreach (string key in array2)
			{
				if (Config.NodesByPlayer.TryGetValue(key, out nodes))
				{
					return true;
				}
			}
		}
		string clearId;
		PermissionLevel permission;
		return (PermissionUtils.TryGetClearId(hub, out clearId) && Config.NodesByPlayer.TryGetValue(clearId, out nodes)) || (TryGetLevel(hub, out permission) && Config.NodesByLevel.TryGetValue(permission, out nodes));
	}

	public static void AssignLevel(string target, PermissionLevel level)
	{
		Config.LevelsByPlayer[target] = level;
		Loader.SaveConfig();
	}

	public static void RemoveLevel(string target)
	{
		if (Config.LevelsByPlayer.Remove(target))
		{
			Loader.SaveConfig();
		}
	}

	public static void AddNodes(string target, params string[] nodes)
	{
		List<string> nodeList = new List<string>();
		if (Config.NodesByPlayer.TryGetValue(target, out var _))
		{
			nodeList.AddRange(nodes);
		}
		nodeList.AddRange(nodes.Where((string node) => !nodeList.Contains(node)));
		nodeList = nodeList.OrderByDescending((string node) => node).ToList();
		Config.NodesByPlayer[target] = nodeList.ToArray();
		Loader.SaveConfig();
	}

	public static void AddNodes(PermissionLevel level, params string[] nodes)
	{
		List<string> nodeList = new List<string>();
		if (Config.NodesByLevel.TryGetValue(level, out var _))
		{
			nodeList.AddRange(nodes);
		}
		nodeList.AddRange(nodes.Where((string node) => !nodeList.Contains(node)));
		nodeList = nodeList.OrderByDescending((string node) => node).ToList();
		Config.NodesByLevel[level] = nodeList.ToArray();
		Loader.SaveConfig();
	}

	public static void RemoveNodes(string target, params string[] nodes)
	{
		List<string> nodeList = new List<string>();
		if (Config.NodesByPlayer.TryGetValue(target, out var _))
		{
			nodeList.AddRange(nodes);
		}
		nodes.ForEach(delegate(string node)
		{
			nodeList.Remove(node);
		});
		nodeList = nodeList.OrderByDescending((string node) => node).ToList();
		Config.NodesByPlayer[target] = nodeList.ToArray();
		Loader.SaveConfig();
	}

	public static void RemoveNodes(PermissionLevel level, params string[] nodes)
	{
		List<string> nodeList = new List<string>();
		if (Config.NodesByLevel.TryGetValue(level, out var _))
		{
			nodeList.AddRange(nodes);
		}
		nodes.ForEach(delegate(string node)
		{
			nodeList.Remove(node);
		});
		nodeList = nodeList.OrderByDescending((string node) => node).ToList();
		Config.NodesByLevel[level] = nodeList.ToArray();
		Loader.SaveConfig();
	}

	[Command("addnodes", new CommandType[]
	{
		CommandType.RemoteAdmin,
		CommandType.GameConsole
	})]
	[Permission(PermissionLevel.Administrator)]
	private static string AddNodesCommand(IPlayer sender, PermissionLevel level, string[] nodes)
	{
		AddNodes(level, nodes);
		return string.Format("Added nodes to {0}: {1}", level, string.Join(",", nodes));
	}

	[Command("removenodes", new CommandType[]
	{
		CommandType.RemoteAdmin,
		CommandType.GameConsole
	})]
	[Permission(PermissionLevel.Administrator)]
	private static string RemoveNodesCommand(IPlayer sender, PermissionLevel level, string[] nodes)
	{
		RemoveNodes(level, nodes);
		return string.Format("Removed nodes from {0}: {1}", level, string.Join(",", nodes));
	}

	[Command("assignlevel", new CommandType[]
	{
		CommandType.RemoteAdmin,
		CommandType.GameConsole
	})]
	[Permission(PermissionLevel.Administrator)]
	private static string AssignLevelCommand(IPlayer sender, PermissionLevel level, string target)
	{
		AssignLevel(target, level);
		return $"Added level {level} to target: {target}";
	}

	[Command("removelevel", new CommandType[]
	{
		CommandType.RemoteAdmin,
		CommandType.GameConsole
	})]
	[Permission(PermissionLevel.Administrator)]
	private static string RemoveLevelCommand(IPlayer sender, string target)
	{
		RemoveLevel(target);
		return "Removed level from target: " + target;
	}
}
