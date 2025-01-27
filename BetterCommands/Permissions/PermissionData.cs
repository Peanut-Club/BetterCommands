using CentralAuth;
using helpers.Results;

namespace BetterCommands.Permissions;

public class PermissionData
{
	public string[] RequiredNodes { get; }

	public PermissionNodeMode NodeMode { get; }

	public PermissionLevel? RequiredLevel { get; }

	public IResult Validate(ReferenceHub player)
	{
		if (player.Mode == ClientInstanceMode.DedicatedServer || player.Mode == ClientInstanceMode.Host)
		{
			return Result.Success();
		}
		if (RequiredNodes != null && RequiredNodes.Length != 0)
		{
			if (!PermissionManager.TryGetNodes(player, out var nodes))
			{
				return Result.Error("Missing permissions!\nNo Required Nodes" + ToString());
			}
			return PermissionUtils.TryValidateNodes(RequiredNodes, nodes, NodeMode) ? Result.Success() : Result.Error("Missing permissions!\nYour Nodes: " + string.Join(", ", nodes) + "\n" + ToString());
		}
		if (RequiredLevel.HasValue && RequiredLevel.Value != 0)
		{
			if (!PermissionManager.TryGetLevel(player, out var permission))
			{
				return Result.Error("Missing permissions!\nNo Permission Level\n" + ToString());
			}
			if (RequiredLevel.Value > permission)
			{
				return Result.Error("Missing permissions!\nYour Level: " + permission.ToString() + "\n" + ToString());
			}
		}
		return Result.Success();
	}

	public PermissionData(string[] reqNodes, PermissionNodeMode permissionNodeMode, PermissionLevel? permissionLevel = null)
	{
		RequiredNodes = reqNodes;
		NodeMode = permissionNodeMode;
		RequiredLevel = permissionLevel;
	}

	public override string ToString()
	{
		string text = "";
		if (RequiredNodes != null && RequiredNodes.Length != 0)
		{
			text = text + "Required Nodes (" + ((NodeMode == PermissionNodeMode.AllOf) ? "all of" : "any of") + "): " + string.Join(", ", RequiredNodes);
		}
		if (RequiredLevel.HasValue)
		{
			if (text != "")
			{
				text += "\n";
			}
			text += $"Required Level: {RequiredLevel.Value}";
		}
		return text;
	}
}
