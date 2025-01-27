using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BetterCommands.Conditions;
using BetterCommands.Parsing;
using BetterCommands.Permissions;
using helpers.Pooling.Pools;
using helpers.Results;
using PluginAPI.Core;
using PluginAPI.Core.Interfaces;
using PluginAPI.Loader;

namespace BetterCommands.Management;

public class CommandData
{
	public MethodInfo TargetMethod { get; }

	public Type DeclaringType { get; }

	public Type SenderType { get; }

	public PluginHandler Plugin { get; }

	public PermissionData Permissions { get; }

	public CommandArgumentData[] Arguments { get; }

	public ConditionData[] Conditions { get; }

	public string Name { get; }

	public string Description { get; }

	public string Usage { get; }

	public string[] Aliases { get; }

	public object Handle { get; }

	public bool IsHidden { get; }

	public CommandData(MethodInfo target, PermissionData permissions, ConditionData[] conditions, CommandArgumentData[] arguments, string name, string description, string[] aliases, bool hidden, object handle)
	{
		TargetMethod = target;
		DeclaringType = target.DeclaringType;
		SenderType = target.GetParameters()[0].ParameterType;
		Plugin = AssemblyLoader.InstalledPlugins.FirstOrDefault((PluginHandler x) => x._entryPoint.DeclaringType.Assembly == target.DeclaringType.Assembly);
		if (Plugin == null)
		{
			Plugin = Loader.Handler;
		}
		Permissions = permissions;
		Arguments = arguments;
		Conditions = conditions;
		Name = name;
		Description = description;
		Aliases = aliases;
		IsHidden = hidden;
		Handle = handle;
		StringBuilder stringBuilder = StringBuilderPool.Pool.Get();
		stringBuilder.AppendLine("<color=#33FF4F>『" + Name + "』</color>");
		for (int i = 0; i < Arguments.Length; i++)
		{
			if (!Arguments[i].IsOptional)
			{
				stringBuilder.AppendLine($"<color=#E3FF33>《{i + 1}》</color>: <color=#FFF333><b>{Arguments[i].Name}<b></color> 〔{Arguments[i].UserName}〕");
			}
			else
			{
				stringBuilder.AppendLine($"<color=#E3FF33>《{i + 1}》 ﹤optional﹥</color>: <color=#FFF333><b>{Arguments[i].Name}</b></color> <color=#33FCFF>〔{Arguments[i].UserName}〕</color>" + "\n    <b>default value: " + (Arguments[i].DefaultValue?.ToString() ?? "none") + "</b>");
			}
		}
		Usage = StringBuilderPool.Pool.PushReturn(stringBuilder);
	}

	public IResult Execute(string clearArgs, ReferenceHub sender)
	{
		if (Permissions != null)
		{
			IResult result = Permissions.Validate(sender);
			if (!result.IsSuccess)
			{
				return Result.Error("Permissions failed:\n" + result.ReadError());
			}
		}
		if (Conditions != null)
		{
			for (int i = 0; i < Conditions.Length; i++)
			{
				IResult result2 = Conditions[i].Validate(sender);
				if (!result2.IsSuccess)
				{
					return Result.Error($"Condition {i} of {Conditions.Length} failed:\n{result2.ReadError()}");
				}
			}
		}
		List<object> list = ListPool<object>.Pool.Get();
		if (SenderType == typeof(ReferenceHub))
		{
			list.Add(sender);
		}
		else if (SenderType == typeof(IPlayer) || SenderType == typeof(Player))
		{
			if (!Player.TryGet(sender, out var player))
			{
				return Result.Error("Failed to retrieve Player instance from ReferenceHub to pass as sender!");
			}
			list.Add(player);
		}
		else
		{
			if (!FactoryManager.FactoryTypes.TryGetValue(SenderType, out var value))
			{
				return Result.Error("Failed to find player factory type for player type: " + SenderType.FullName);
			}
			if (!FactoryManager.PlayerFactories.TryGetValue(value, out var value2))
			{
				return Result.Error("Failed to find player factory for factory type: " + value.FullName);
			}
			Player orAdd = value2.GetOrAdd((IGameComponent)(object)sender);
			if (orAdd == null)
			{
				return Result.Error("Factory " + value.FullName + " supplied an invalid result");
			}
			list.Add(orAdd);
		}
		IResult result3 = CommandArgumentParser.Parse(this, clearArgs, sender);
		if (!result3.TryReadResult<object[]>(failOnError: true, out var value3))
		{
			return Result.Error("Failed to execute command " + Name + ":\n" + result3.ReadError());
		}
		for (int j = 0; j < value3.Length; j++)
		{
			list.Add(value3[j]);
		}
		object[] parameters = list.ToArray();
		ListPool<object>.Pool.Push(list);
		try
		{
			object obj = TargetMethod?.Invoke(Handle, parameters);
			if (obj == null)
			{
				return Result.Success("Command succesfully executed without output.");
			}
			if (obj is IResult result4 && result4.TryReadResult<string>(failOnError: true, out var value4))
			{
				return Result.Success(value4);
			}
			object obj2 = obj;
			object obj3 = obj2;
			if (!(obj3 is string value5))
			{
				if (obj3 is IEnumerable enumerable)
				{
					return Result.Success(string.Join("\n", enumerable));
				}
				return Result.Success($"Command executed succesfully, but it returned an unsupported response type: {obj} ({obj.GetType().FullName})");
			}
			return Result.Success(value5);
		}
		catch (Exception ex)
		{
			return Result.Error($"Failed to execute command {Name}:\n{ex}", ex);
		}
	}
}
