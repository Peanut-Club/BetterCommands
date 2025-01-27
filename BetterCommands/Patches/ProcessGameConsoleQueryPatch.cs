using System;
using System.Linq;
using BetterCommands.Management;
using HarmonyLib;
using helpers.Extensions;
using PluginAPI.Events;
using RemoteAdmin;

namespace BetterCommands.Patches;

[HarmonyPatch(typeof(QueryProcessor), "ProcessGameConsoleQuery")]
public static class ProcessGameConsoleQueryPatch
{
	public static bool Prefix(QueryProcessor __instance, string query)
	{
		string[] array = query.Trim().Split(QueryProcessor.SpaceArray, 512, StringSplitOptions.RemoveEmptyEntries);
		if (!EventManager.ExecuteEvent(new PlayerGameConsoleCommandEvent(__instance._hub, array[0], array.Skip(1).ToArray())))
		{
			return false;
		}
		if (CommandManager.TryExecute(string.Join(" ", array), __instance._hub, CommandType.PlayerConsole, out var response))
		{
			__instance._hub.gameConsoleTransmission.SendToClient("SYSTEM#" + response.RemoveHtmlTags(), "yellow");
			return false;
		}
		if (QueryProcessor.DotCommandHandler.TryGetCommand(array[0], out var command))
		{
			try
			{
				string response2;
				bool result = command.Execute(array.Segment(1), __instance._sender, out response2);
				if (!EventManager.ExecuteEvent(new PlayerGameConsoleCommandExecutedEvent(__instance._hub, array[0], array.Skip(1).ToArray(), result, response2)))
				{
					return false;
				}
				__instance._hub.gameConsoleTransmission.SendToClient(array[0].ToUpper() + "#" + response2.RemoveHtmlTags(), "");
			}
			catch (Exception arg)
			{
				string text = $"Command execution failed! Error:\n{arg}";
				if (!EventManager.ExecuteEvent(new PlayerGameConsoleCommandExecutedEvent(__instance._hub, array[0], array.Skip(1).ToArray(), result: false, text)))
				{
					return false;
				}
				__instance._hub.gameConsoleTransmission.SendToClient(array[0].ToUpper() + "#" + text, "");
			}
			return false;
		}
		string text2 = "Command not found.";
		if (!EventManager.ExecuteEvent(new PlayerGameConsoleCommandExecutedEvent(__instance._hub, array[0], array.Skip(1).ToArray(), result: false, text2)))
		{
			return false;
		}
		__instance._hub.gameConsoleTransmission.SendToClient("SYSTEM#" + text2, "red");
		return false;
	}
}
