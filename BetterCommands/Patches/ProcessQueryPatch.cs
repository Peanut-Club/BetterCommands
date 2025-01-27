using System;
using System.Linq;
using BetterCommands.Management;
using HarmonyLib;
using helpers.Extensions;
using helpers.Network.Requests;
using PluginAPI.Events;
using RemoteAdmin;
using RemoteAdmin.Communication;

namespace BetterCommands.Patches;

[HarmonyPatch(typeof(CommandProcessor), "ProcessQuery")]
public static class ProcessQueryPatch
{
	public static bool Prefix(string q, CommandSender sender, ref string __result)
	{
		__result = "";
		if (q.StartsWith("$"))
		{
			string[] array = q.Remove(0, 1).Split(new char[1] { ' ' });
			if (array.Length == 0)
			{
				__result = null;
				return false;
			}
			if (!int.TryParse(array[0], out var result))
			{
				__result = null;
				return false;
			}
			if (CommunicationProcessor.ServerCommunication.TryGetValue(result, out var value))
			{
				value.ReceiveData(sender, string.Join(" ", array.Skip(1)));
			}
			__result = null;
			return false;
		}
		PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
		if (q.StartsWith("/", StringComparison.Ordinal) && q.Length > 1)
		{
			int length = q.Length;
			int num = 1;
			int length2 = length - num;
			if (string.IsNullOrEmpty(q.Substring(num, length2).TrimStart(new char[1] { '$' })))
			{
				sender?.Print("Command cant be empty!", ConsoleColor.Green);
				__result = "Command cant be empty!";
				return false;
			}
		}
		string[] array2 = q.Trim().Split(new char[1]{ ' ' }, count: 512, options: StringSplitOptions.RemoveEmptyEntries);
		if (!EventManager.ExecuteEvent(new RemoteAdminCommandEvent(sender, array2[0], array2.Skip(1).ToArray())))
		{
			__result = null;
			return false;
		}
		if (playerCommandSender == null)
		{
			if (CommandManager.TryExecute(string.Join(" ", array2), ReferenceHub.HostHub, CommandType.RemoteAdmin, out __result))
			{
				__result = __result.RemoveHtmlTags();
                if (!EventManager.ExecuteEvent(new RemoteAdminCommandExecutedEvent(sender, array2[0], array2.Skip(1).ToArray(), result: true, __result))) {
                    return false;
                }
                sender.RaReply(__result, success: true, logToConsole: true, string.Empty);
                return false;
			}
		}
		else if (CommandManager.TryExecute(string.Join(" ", array2), playerCommandSender.ReferenceHub, CommandType.RemoteAdmin, out __result))
		{
            if (!EventManager.ExecuteEvent(new RemoteAdminCommandExecutedEvent(sender, array2[0], array2.Skip(1).ToArray(), result: true, __result))) {
                return false;
            }
			sender.RaReply(__result, success: true, logToConsole: true, string.Empty);
			return false;
		}
		if (CommandProcessor.RemoteAdminCommandHandler.TryGetCommand(array2[0], out var command))
		{
			try
			{
				string response;
				bool flag = command.Execute(array2.Segment(1), sender, out response);
				if (!EventManager.ExecuteEvent(new RemoteAdminCommandExecutedEvent(sender, array2[0], array2.Skip(1).ToArray(), flag, response)))
				{
					__result = null;
					return false;
				}
				if (!string.IsNullOrWhiteSpace(response))
				{
					sender.RaReply(array2[0].ToUpper() + "#" + response, flag, logToConsole: true, string.Empty);
				}
				__result = response;
				return false;
			}
			catch (Exception arg)
			{
				__result = $"Command execution failed!\n{arg}";
				return false;
			}
		}
		if (!EventManager.ExecuteEvent(new RemoteAdminCommandExecutedEvent(sender, array2[0], array2.Skip(1).ToArray(), result: false, "Unknown command!")))
		{
			__result = null;
			return false;
		}
		sender.RaReply("SYS#Unknown command!", success: false, logToConsole: true, string.Empty);
		__result = "Unknown command!";
		return false;
	}
}
