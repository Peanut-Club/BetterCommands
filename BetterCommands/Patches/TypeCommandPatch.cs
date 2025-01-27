using System;
using System.Linq;
using BetterCommands.Management;
using GameCore;
using HarmonyLib;
using helpers.Extensions;
using PluginAPI.Events;
using RemoteAdmin;
using UnityEngine;

namespace BetterCommands.Patches;

[HarmonyPatch(typeof(GameCore.Console), "TypeCommand")]
public static class TypeCommandPatch
{
	public static bool Prefix(GameCore.Console __instance, string cmd, ref string __result, CommandSender sender = null)
	{
		if (sender == null)
		{
			sender = ServerConsole.Scs;
		}
		bool flag = cmd.StartsWith("@");
		if ((cmd.StartsWith("/") || flag) && cmd.Length > 1)
		{
			string text = (flag ? cmd : cmd.Substring(1));
			if (!flag)
			{
				text = text.TrimStart(new char[1] { '$' });
				if (string.IsNullOrWhiteSpace(text))
				{
					sender?.Print("Command can't be empty!", ConsoleColor.Red);
					__result = "Command can't be empty!";
					return false;
				}
			}
			__result = CommandProcessor.ProcessQuery(text, sender);
			return false;
		}
		string[] array = cmd.Trim().Split(QueryProcessor.SpaceArray, 512, StringSplitOptions.RemoveEmptyEntries);
		if (!EventManager.ExecuteEvent(new ConsoleCommandEvent(sender, array[0], array.Skip(1).ToArray())))
		{
			__result = null;
			return false;
		}
		if (CommandManager.TryExecute(string.Join(" ", array), ReferenceHub.HostHub, CommandType.GameConsole, out var response))
		{
			__result = response.RemoveHtmlTags();
			sender.Print(__result, ConsoleColor.White);
			return false;
		}
		cmd = array[0];
		if (__instance.ConsoleCommandHandler.TryGetCommand(cmd, out var command))
		{
			string response2 = "";
			try
			{
				bool flag2 = command.Execute(array.Segment(1), sender, out response2);
				if (!EventManager.ExecuteEvent(new ConsoleCommandExecutedEvent(sender, array[0], array.Skip(1).ToArray(), flag2, response2)))
				{
					response2 = null;
					return false;
				}
				sender?.Print(response2, flag2 ? ConsoleColor.Green : ConsoleColor.Red);
			}
			catch (Exception arg)
			{
				response2 = $"Command execution failed! Error:\n{arg}";
				if (!EventManager.ExecuteEvent(new ConsoleCommandExecutedEvent(sender, array[0], array.Skip(1).ToArray(), result: false, response2)))
				{
					__result = null;
					return false;
				}
				sender?.Print(response2, ConsoleColor.Red);
			}
			__result = response2;
			return false;
		}
		string text2 = "Command " + cmd + " does not exist!";
		if (!EventManager.ExecuteEvent(new ConsoleCommandExecutedEvent(sender, array[0], array.Skip(1).ToArray(), result: false, text2)))
		{
			__result = null;
			return false;
		}
		sender?.Print(text2, ConsoleColor.DarkYellow, new Color(255f, 180f, 0f, 255f));
		__result = text2;
		return false;
	}
}
