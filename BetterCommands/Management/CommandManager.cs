using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BetterCommands.Conditions;
using BetterCommands.Parsing;
using BetterCommands.Permissions;
using helpers;
using helpers.Extensions;
using helpers.Results;
using PluginAPI.Core;
using PluginAPI.Core.Interfaces;
using RemoteAdmin;
using Utils.NonAllocLINQ;

namespace BetterCommands.Management;

public static class CommandManager
{
	private static readonly Dictionary<CommandType, HashSet<CommandData>> _commandsByType = new Dictionary<CommandType, HashSet<CommandData>>
	{
		[CommandType.RemoteAdmin] = new HashSet<CommandData>(),
		[CommandType.GameConsole] = new HashSet<CommandData>(),
		[CommandType.PlayerConsole] = new HashSet<CommandData>()
	};

	private static readonly BindingFlags AllBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	public static IReadOnlyDictionary<CommandType, HashSet<CommandData>> Commands => _commandsByType;

	public static void Register()
	{
		Register(Assembly.GetCallingAssembly());
	}

	public static void Register(Assembly assembly)
	{
		assembly.GetTypes().ForEach(delegate(Type x)
		{
			Register(x, null);
		});
	}

	public static void Register(Type type, object handle)
	{
		type.GetMethods(AllBindingFlags).ForEach(delegate(MethodInfo x)
		{
			Register(x, handle);
		});
	}

	public static void Register(MethodInfo method, object handle)
	{
		try
		{
			if (!method.TryGetAttribute<CommandAttribute>(out var attributeValue))
			{
				return;
			}
			List<CommandArgumentData> args = new List<CommandArgumentData>();
			ParameterInfo[] parameters = method.GetParameters();
			if (parameters == null || !parameters.Any() || (!Reflection.HasInterface<IPlayer>(parameters[0].ParameterType) && parameters[0].ParameterType != typeof(ReferenceHub) && parameters[0].ParameterType != typeof(IPlayer)))
			{
				PluginAPI.Core.Log.Warning("Plugin " + method.DeclaringType.Assembly.GetName().Name + " has a method (" + method.DeclaringType.FullName + "::" + method.Name + ") marked as a command, but it's arguments are invalid! The first parameter has to be either an IPlayer implementation or ReferenceHub!", "Command Manager");
				return;
			}
			ParameterInfo[] array = parameters.Skip(1).ToArray();
			if (array.Any())
			{
				array.ForEach(delegate(ParameterInfo arg)
				{
					LookingAtAttribute customAttribute = arg.GetCustomAttribute<LookingAtAttribute>();
					ValueRestrictionAttribute customAttribute2 = arg.GetCustomAttribute<ValueRestrictionAttribute>();
					if (customAttribute == null)
					{
						args.Add(new CommandArgumentData(arg.ParameterType, arg.Name, arg.HasDefaultValue, lookingAt: false, 0f, 0, arg.DefaultValue, customAttribute2?.GetMode() ?? ValueRestrictionMode.None, customAttribute2?.GetValues() ?? Array.Empty<object>()));
					}
					else
					{
						args.Add(new CommandArgumentData(arg.ParameterType, arg.Name, arg.HasDefaultValue, lookingAt: true, customAttribute.GetDistance(), customAttribute.GetMask(), arg.DefaultValue, customAttribute2?.GetMode() ?? ValueRestrictionMode.None, customAttribute2?.GetValues() ?? Array.Empty<object>()));
					}
				});
			}
			CommandArgumentData[] arguments = args.ToArray();
			ConditionData[] conditions = Array.Empty<ConditionData>();
			string[] aliases = Array.Empty<string>();
			string description = "No description.";
			PermissionData permissions = null;
			if (method.TryGetAttribute<PermissionAttribute>(out var attributeValue2))
			{
				permissions = new PermissionData(attributeValue2.RequiredNodes, attributeValue2.NodeMode, attributeValue2.RequiredLevel);
			}
			if (method.TryGetAttribute<DescriptionAttribute>(out var attributeValue3))
			{
				description = attributeValue3.Description;
			}
			if (method.TryGetAttribute<CommandAliasesAttribute>(out var attributeValue4))
			{
				aliases = attributeValue4.Aliases ?? Array.Empty<string>();
			}
			IEnumerable<ConditionAttribute> customAttributes = method.GetCustomAttributes<ConditionAttribute>();
			if (customAttributes.Any())
			{
				List<ConditionData> conditionList = new List<ConditionData>();
				customAttributes.ForEach(delegate(ConditionAttribute attribute)
				{
					conditionList.Add(new ConditionData(attribute.Flags, attribute.ConditionObject));
				});
				conditions = conditionList.ToArray();
			}
			CommandData commandData = new CommandData(method, permissions, conditions, arguments, attributeValue.Name, description, aliases, attributeValue.IsHidden, handle);
			if (attributeValue.Types.Contains(CommandType.RemoteAdmin))
			{
				TryRegister(commandData, CommandType.RemoteAdmin);
			}
			if (attributeValue.Types.Contains(CommandType.GameConsole))
			{
				TryRegister(commandData, CommandType.GameConsole);
			}
			if (attributeValue.Types.Contains(CommandType.PlayerConsole))
			{
				TryRegister(commandData, CommandType.PlayerConsole);
			}
		}
		catch (Exception arg2)
		{
			PluginAPI.Core.Log.Error($"Failed to register {method.Name} of {method.DeclaringType.FullName}: {arg2}", "Command Manager");
		}
	}

	public static bool TryRegister(CommandData commandData, CommandType commandType)
	{
		if (TryGetCommand(commandData.Name, commandType, out var _))
		{
			PluginAPI.Core.Log.Warning("Plugin " + commandData.Plugin.PluginName + " tried to register an already existing command! (" + commandData.Name + ")", "Command Manager");
			return false;
		}
		_commandsByType[commandType].Add(commandData);
		return true;
	}

	public static bool TryUnregister(string cmdName, CommandType type)
	{
		if (!TryGetCommand(cmdName, type, out var commandData))
		{
			PluginAPI.Core.Log.Warning($"Something tried to unregister an unknown command of type {type}: {cmdName}", "Command Manager");
			return false;
		}
		_commandsByType[type].Remove(commandData);
		return true;
	}

	public static bool TryGetCommand(string arg, CommandType commandType, out CommandData commandData)
	{
		commandData = null;
		arg = arg.Trim().ToLowerInvariant();
		foreach (CommandData item in _commandsByType[commandType])
		{
			if (item.Name.ToLower() == arg)
			{
				commandData = item;
				return true;
			}
			if (item.Aliases == null || !item.Aliases.Any())
			{
				continue;
			}
			string[] aliases = item.Aliases;
			string[] array = aliases;
			foreach (string text in array)
			{
				if (text.ToLower() == arg)
				{
					commandData = item;
					return true;
				}
			}
		}
		return false;
	}

	public static bool TryExecute(string argString, ReferenceHub sender, CommandType commandType, out string response)
	{
		string[] array = argString.Split(new char[1] { ' ' });
		string arg = array[0];
		response = null;
		if (!TryGetCommand(arg, commandType, out var commandData))
		{
			return false;
		}
		response = commandData.Name.ToUpper() + "#\n<color=#33FFD7>";
		IResult result = commandData.Execute(string.Join(" ", array.Skip(1)), sender);
		if (!result.TryReadResult<string>(failOnError: true, out var value))
		{
			string error = result.ReadError();
			Exception ex = result.ReadException();
			ColorUtils.ColorMatchError(ref error, isException: false);
			response = response + "Command execution <color=red>failed</color>!\n<color=red>" + error + "</color>";
			if (ex != null)
			{
				string error2 = ex.ToString();
				ColorUtils.ColorMatchError(ref error2, isException: true);
				response = response + "Exception:\n" + error2;
				response += "</color>";
			}
			if (commandType != 0)
			{
				response = response.RemoveHtmlTags();
			}
			return true;
		}
		response += value;
		response += "</color>";
		if (commandType != 0)
		{
			response = response.RemoveHtmlTags();
		}
		return true;
	}

	public static void UnregisterAll()
	{
		_commandsByType.ForEach(delegate(KeyValuePair<CommandType, HashSet<CommandData>> pair)
		{
			pair.Value.Clear();
		});
	}

	internal static void Synchronize(List<QueryProcessor.CommandData> commands)
	{
		if (!_commandsByType.TryGetValue(CommandType.RemoteAdmin, out var value) || !value.Any())
		{
			return;
		}
		value.ForEach(delegate(CommandData x)
		{
			QueryProcessor.CommandData commandData = default(QueryProcessor.CommandData);
			commandData.Command = x.Name;
			commandData.Description = x.Description;
			commandData.Hidden = x.IsHidden;
			commandData.Usage = null;
			commandData.AliasOf = null;
			QueryProcessor.CommandData item = commandData;
			commands.Add(item);
			if (x.Aliases.Any())
			{
				string[] aliases = x.Aliases;
				string[] array = aliases;
				foreach (string command in array)
				{
					commands.Add(new QueryProcessor.CommandData
					{
						Command = command,
						Description = null,
						Usage = null,
						Hidden = item.Hidden,
						AliasOf = item.Command
					});
				}
			}
		});
	}
}
