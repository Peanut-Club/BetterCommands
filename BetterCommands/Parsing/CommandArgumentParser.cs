using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BetterCommands.Arguments;
using BetterCommands.Arguments.Effects;
using BetterCommands.Arguments.Prefabs;
using BetterCommands.Management;
using BetterCommands.Parsing.Parsers;
using helpers;
using helpers.Parsers.String;
using helpers.Pooling.Pools;
using helpers.Results;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using Mirror;
using PluginAPI.Core.Interfaces;
using UnityEngine;

namespace BetterCommands.Parsing;

public static class CommandArgumentParser
{
	private static readonly Dictionary<Type, ICommandArgumentParser> _knownParsers;

	private static readonly Dictionary<Type, Type> _parsingTypeCache;

	static CommandArgumentParser()
	{
		_knownParsers = new Dictionary<Type, ICommandArgumentParser>();
		_parsingTypeCache = new Dictionary<Type, Type>();
		AdminToyParser.Register();
		CollectionParser.Register();
		SimpleParser.Register();
		AddParser<PlayerParser>(typeof(IPlayer));
		AddParser<DoorParser>(typeof(DoorVariant));
		AddParser<ReferenceHubParser>(typeof(ReferenceHub));
		AddParser<RoomIdentifierParser>(typeof(RoomIdentifier));
		AddParser<EffectParser>(typeof(EffectData));
		AddParser<GameObjectParser>(typeof(GameObject));
		AddParser<NetworkIdentityParser>(typeof(NetworkIdentity));
		AddParser<PrefabParser>(typeof(PrefabData));
	}

	public static ICommandArgumentParser GetParser(Type type)
	{
		ICommandArgumentParser commandArgumentParser;
		return (!TryGetParser(type, out commandArgumentParser)) ? null : commandArgumentParser;
	}

	public static bool TryGetParser(Type argType, out ICommandArgumentParser commandArgumentParser)
	{
		if (!_parsingTypeCache.ContainsKey(argType))
		{
			Type key = argType;
			if (Reflection.HasInterface<IPlayer>(argType))
			{
				argType = typeof(IPlayer);
			}
			else if (Reflection.HasType<DoorVariant>(argType))
			{
				argType = typeof(DoorVariant);
			}
			else if (argType.IsEnum)
			{
				argType = typeof(Enum);
			}
			else if (argType.IsArray)
			{
				argType = typeof(Array);
			}
			else if (Reflection.HasInterface<IDictionary>(argType))
			{
				argType = typeof(IDictionary);
			}
			else if (Reflection.HasInterface<IEnumerable>(argType) && argType != typeof(string))
			{
				argType = typeof(IEnumerable);
			}
			_parsingTypeCache[key] = argType;
		}
		else
		{
			argType = _parsingTypeCache[argType];
		}
		return _knownParsers.TryGetValue(argType, out commandArgumentParser) && commandArgumentParser != null;
	}

	public static TParser AddParser<TParser>(Type parsedType) where TParser : ICommandArgumentParser, new()
	{
		return (TParser)AddParser(new TParser(), parsedType);
	}

	public static ICommandArgumentParser AddParser(Type parserType, Type parsedType)
	{
		return AddParser(Activator.CreateInstance(parserType) as ICommandArgumentParser, parsedType);
	}

	public static ICommandArgumentParser AddParser(ICommandArgumentParser commandArgumentParser, Type type)
	{
		_knownParsers[type] = commandArgumentParser;
		return commandArgumentParser;
	}

	public static IResult Parse(CommandData command, string input, ReferenceHub sender)
	{
		try
		{
			IResult result = StringParser.Parse(input);
			if (!result.IsSuccess)
			{
				return Result.Error("Failed to parse string into arguments: " + result.ReadError());
			}
			string[] array = result.ReadResult<string[]>();
			List<object> list = ListPool<object>.Pool.Get();
			for (int i = 0; i < command.Arguments.Length; i++)
			{
				CommandArgumentData commandArgumentData = command.Arguments[i];
				if (commandArgumentData.IsLookingAt)
				{
					if (!ArgumentUtils.TryGetLookingAt(sender, commandArgumentData.LookingAtDistance, commandArgumentData.LookingAtMask, commandArgumentData.Type, out var hit))
					{
						return Result.Error($"Failed to find a valid object of type {commandArgumentData.Type.FullName} in radius of {commandArgumentData.LookingAtDistance} in mask {commandArgumentData.LookingAtMask} of a looking-at argument at index {i}");
					}
					list.Add(hit);
					continue;
				}
				if (commandArgumentData.Type == typeof(CommandArguments))
				{
					CommandArguments commandArguments = new CommandArguments();
					commandArguments.Parse(array[i]);
					list.Add(commandArguments);
					continue;
				}
				if (i >= array.Length)
				{
					if (commandArgumentData.IsOptional)
					{
						list.Add(commandArgumentData.DefaultValue);
						continue;
					}
					if (!(commandArgumentData.Type == typeof(CommandArguments)))
					{
						return Result.Error("<color=red>Missing arguments!</color>\n" + command.Usage);
					}
					list.Add(new CommandArguments());
					continue;
				}
				string value = array[i];
				IResult result2 = commandArgumentData.Parse(value);
				if (!result2.IsSuccess)
				{
					return Result.Error($"Failed to parse argument {commandArgumentData.Name} at index {i}:\n      {result2.ReadError()}");
				}
				if (commandArgumentData.RestrictionMode != 0 && commandArgumentData.RestrictedValues != null && commandArgumentData.RestrictedValues.Any())
				{
					if (commandArgumentData.RestrictionMode == ValueRestrictionMode.Blacklist)
					{
						if (commandArgumentData.RestrictedValues.Contains(result2.Value))
						{
							return Result.Error(string.Format("Value {0} is restricted from parameter {1} (index: {2}) [blacklisted values: {3}", result2.Value, commandArgumentData.Name, i, string.Join(", ", commandArgumentData.RestrictedValues)));
						}
					}
					else if (!commandArgumentData.RestrictedValues.Contains(result2.Value))
					{
						return Result.Error(string.Format("Value {0} is restricted from parameter {1} (index: {2}) [whitelisted values: {3}", result2.Value, commandArgumentData.Name, i, string.Join(", ", commandArgumentData.RestrictedValues)));
					}
				}
				list.Add(result2.Value);
			}
			IResult result3 = Result.Success(list.ToArray());
			ListPool<object>.Pool.Push(list);
			return result3;
		}
		catch (Exception ex)
		{
			return Result.Error(ex.ToString(), ex);
		}
	}
}
