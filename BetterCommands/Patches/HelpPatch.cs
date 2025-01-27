using System;
using CommandSystem;
using CommandSystem.Commands.Shared;
using HarmonyLib;

namespace BetterCommands.Patches;

[HarmonyPatch(typeof(HelpCommand), "Execute")]
public static class HelpPatch
{
	public static bool Prefix(HelpCommand __instance, ArraySegment<string> arguments, ICommandSender sender, ref bool __result, out string response)
	{
		if (arguments.Count <= 0)
		{
			response = __instance.GetCommandList(__instance._commandHandler, "Command list:");
			__result = true;
			return false;
		}
		if (__instance._commandHandler.TryGetCommand(arguments.At(0), out var command))
		{
			string text = command.Command;
			ArraySegment<string> arraySegment = arguments.Segment(1);
			ICommand command2;
			while (arraySegment.Count != 0 && command is ICommandHandler commandHandler && commandHandler.TryGetCommand(arraySegment.At(0), out command2))
			{
				arraySegment = arraySegment.Segment(1);
				command = command2;
				text = text + " " + command2.Command;
			}
			IHelpProvider helpProvider = command as IHelpProvider;
			response = text + " - " + ((helpProvider != null) ? helpProvider.GetHelp(arraySegment) : command.Description);
			if (command.Aliases != null && command.Aliases.Length != 0)
			{
				response = response + "\nAliases: " + string.Join(", ", command.Aliases);
			}
			if (command is ICommandHandler handler)
			{
				response += __instance.GetCommandList(handler, "\nSubcommand list:");
			}
			try
			{
				Type type = command.GetType();
				if (type != null)
				{
					response = response + "\nImplemented in: " + type.Assembly.GetName().Name + ": " + type.FullName;
				}
			}
			catch
			{
			}
			__result = true;
			return false;
		}
		response = "Help for " + arguments.At(0) + " isn't available!";
		__result = false;
		return false;
	}
}
