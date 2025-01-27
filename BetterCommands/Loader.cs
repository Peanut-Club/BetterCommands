using System;
using System.Linq;
using System.Reflection;
using System.Text;
using BetterCommands.Management;
using BetterCommands.Permissions;
using HarmonyLib;
using helpers;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Interfaces;
using PluginAPI.Enums;
using PluginAPI.Loader;

namespace BetterCommands;

public class Loader
{
	[PluginConfig]
	public Config ConfigInstance;

	public PluginHandler HandlerInstance;

	public Harmony HarmonyInstance;

	public static Config Config => Instance.ConfigInstance;

	public static PluginHandler Handler => Instance.HandlerInstance;

	public static Harmony Harmony => Instance.HarmonyInstance;

	public static Loader Instance { get; private set; }

	[PluginEntryPoint("BetterCommands", "1.2.0", "Introduces a new command system for plugins to use.", "marchellc_")]
	[PluginPriority(LoadPriority.Lowest)]
	public void Load()
	{
		Instance = this;
		HandlerInstance = PluginHandler.Get(this);
		HarmonyInstance = new Harmony("marchellc_.bettercmds");
		HarmonyInstance.PatchAll();
		Reload();
	}

	[PluginReload]
	public void Reload()
	{
		CommandManager.UnregisterAll();
		LoadConfig();
		PluginAPI.Core.Log.Info("Searching for commands ..", "Better Commands");
		CommandManager.Register(HandlerInstance._entryPoint.DeclaringType.Assembly);
		foreach (Assembly key in AssemblyLoader.Plugins.Keys)
		{
			if (key != Assembly.GetExecutingAssembly())
			{
				CommandManager.Register(key);
			}
			Type[] types = key.GetTypes();
			Type[] array = types;
			foreach (Type type in array)
			{
				MethodInfo[] methods = type.GetMethods();
				MethodInfo[] array2 = methods;
				foreach (MethodInfo methodInfo in array2)
				{
					if (methodInfo.Name == "RegisterBetterCommandsIndependent" && methodInfo.IsStatic)
					{
						methodInfo.Invoke(null, null);
					}
				}
			}
		}
		PluginAPI.Core.Log.Info($"Search completed (found {CommandManager.Commands[CommandType.RemoteAdmin].Count} remote admin commands; {CommandManager.Commands[CommandType.GameConsole].Count} console commands and {CommandManager.Commands[CommandType.PlayerConsole].Count} player commands).", "Better Commands");
	}

	[PluginUnload]
	public void Unload()
	{
		SaveConfig();
	}

	public static void SaveConfig()
	{
		Handler.SaveConfig(Instance, "ConfigInstance");
	}

	public static void LoadConfig()
	{
		Handler.LoadConfig(Instance, "ConfigInstance");
	}

	[Command("bcreload", new CommandType[]
	{
		CommandType.RemoteAdmin,
		CommandType.GameConsole
	})]
	[Permission(PermissionLevel.Administrator)]
	private static string ReloadCommand(IPlayer sender)
	{
		Instance.Reload();
		return "Reloaded Better Commands!";
	}

	[Command("values", new CommandType[]
	{
		CommandType.RemoteAdmin,
		CommandType.GameConsole,
		CommandType.PlayerConsole
	})]
	[Description("Displays a list of values.")]
	private static string ValuesCommand(ReferenceHub sender, string type)
	{
		if (!Reflection.TryLoadType(type, out var type2))
		{
			return "Failed to load that type.";
		}
		if (!type2.IsEnum)
		{
			return "Result type is not an enum.";
		}
		IOrderedEnumerable<Enum> values = from Enum x in Enum.GetValues(type2)
			orderby x.ToString()
			select x;
		StringBuilder sb = new StringBuilder();
		values.ForEach(delegate(Enum val)
		{
			sb.AppendLine($"[{Convert.ChangeType(val, val.GetTypeCode())}] {val}");
		});
		return sb.ToString();
	}
}
