using System.Linq;
using Compendium;
using Compendium.Staff;

namespace BetterCommands.Permissions;

public static class PermissionUtils
{
	public static bool TryGetGroupKeys(ReferenceHub hub, out string[] groupKeys)
	{
		groupKeys = null;
		if (StaffHandler.Members.TryGetValue(hub.UserId(), out groupKeys))
		{
			return true;
		}
		if (ServerStatic.PermissionsHandler == null || !ServerStatic.PermissionsHandler._members.TryGetValue(hub.UserId(), out var value))
		{
			return false;
		}
		groupKeys = new string[1] { value };
		return true;
	}

	public static bool TryGetClearId(ReferenceHub hub, out string clearId)
	{
		string[] array = hub.UserId().Split(new char[1] { '@' });
		clearId = ((array.Length <= 1) ? null : array[0]);
		return !string.IsNullOrWhiteSpace(clearId);
	}

	public static bool TryValidateNodes(string[] nodes, string[] availableNodes, PermissionNodeMode nodeMode)
	{
		if (nodeMode == PermissionNodeMode.AllOf)
		{
			if (nodes.Length != availableNodes.Length)
			{
				return false;
			}
			for (int i = 0; i < nodes.Length; i++)
			{
				if (!ContainsNode(nodes[i], availableNodes))
				{
					return false;
				}
			}
			return true;
		}
		return nodes.Any((string x) => ContainsNode(x, availableNodes));
	}

	public static bool ContainsNode(string node, string[] availableNodes)
	{
		if (availableNodes.Any((string x) => x == "*") || availableNodes.Any((string x) => x == node))
		{
			return true;
		}
		string[] array = node.Split(new char[1] { '.' });
		if (array.Length > 1)
		{
			string[] array2 = array;
			foreach (string curNode in array2)
			{
				if (availableNodes.Any((string x) => x == curNode + ".*") || availableNodes.Any((string x) => x == "*." + curNode))
				{
					return true;
				}
			}
		}
		return false;
	}
}
