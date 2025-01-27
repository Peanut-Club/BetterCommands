using System.Collections.Generic;
using System.ComponentModel;

namespace BetterCommands.Permissions;

public class PermissionConfig
{
	[System.ComponentModel.Description("Configure which level has which permission nodes.")]
	public Dictionary<PermissionLevel, string[]> NodesByLevel { get; set; } = new Dictionary<PermissionLevel, string[]> { [PermissionLevel.Administrator] = new string[1] { "*" } };


	[System.ComponentModel.Description("Configure which player has which permission nodes.")]
	public Dictionary<string, string[]> NodesByPlayer { get; set; } = new Dictionary<string, string[]> { ["RandomId@steam"] = new string[1] { "*" } };


	[System.ComponentModel.Description("Configure which player has which permission level.")]
	public Dictionary<string, PermissionLevel> LevelsByPlayer { get; set; } = new Dictionary<string, PermissionLevel> { ["RandomId@steam"] = PermissionLevel.Administrator };

}
