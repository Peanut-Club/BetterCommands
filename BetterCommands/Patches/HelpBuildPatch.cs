using BetterCommands.Management;
using CommandSystem;
using CommandSystem.Commands.Shared;
using HarmonyLib;

namespace BetterCommands.Patches;

[HarmonyPatch(typeof(HelpCommand), "GetCommandList")]
public static class HelpBuildPatch
{
	public static bool Prefix(HelpCommand __instance, ICommandHandler handler, string header, ref string __result)
	{
		__instance._helpBuilder.Clear();
		__instance._helpBuilder.Append(header);
		foreach (ICommand allCommand in handler.AllCommands)
		{
			if (!(allCommand is IHiddenCommand))
			{
				__instance._helpBuilder.AppendLine();
				__instance._helpBuilder.Append(allCommand.Command);
				__instance._helpBuilder.Append(" - ");
				__instance._helpBuilder.Append(allCommand.Description);
				if (allCommand.Aliases != null && allCommand.Aliases.Length != 0)
				{
					__instance._helpBuilder.Append(" - Aliases: ");
					__instance._helpBuilder.Append(string.Join(", ", allCommand.Aliases));
				}
			}
		}
		CommandType? commandType = null;
		if (!(handler is RemoteAdminCommandHandler))
		{
			if (!(handler is GameConsoleCommandHandler))
			{
				if (handler is ClientCommandHandler)
				{
					commandType = CommandType.PlayerConsole;
				}
			}
			else
			{
				commandType = CommandType.GameConsole;
			}
		}
		else
		{
			commandType = CommandType.RemoteAdmin;
		}
		if (commandType.HasValue && CommandManager.Commands.TryGetValue(commandType.Value, out var value))
		{
			foreach (CommandData item in value)
			{
				if (!item.IsHidden)
				{
					__instance._helpBuilder.AppendLine();
					__instance._helpBuilder.Append(item.Name);
					__instance._helpBuilder.Append(" - ");
					__instance._helpBuilder.Append(item.Description);
					if (item.Aliases != null && item.Aliases.Length != 0)
					{
						__instance._helpBuilder.Append(" - Aliases: ");
						__instance._helpBuilder.Append(string.Join(", ", item.Aliases));
					}
				}
			}
		}
		__result = __instance._helpBuilder.ToString();
		return false;
	}
}
