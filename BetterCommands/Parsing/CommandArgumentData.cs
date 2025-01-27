using System;
using helpers.Results;
using PluginAPI.Core;

namespace BetterCommands.Parsing;

public class CommandArgumentData
{
	private ICommandArgumentParser _parser;

	public Type Type { get; }

	public ICommandArgumentParser Parser => _parser;

	public ValueRestrictionMode RestrictionMode { get; }

	public object[] RestrictedValues { get; }

	public string Name { get; }

	public string UserName { get; }

	public bool IsOptional { get; }

	public bool IsLookingAt { get; }

	public object DefaultValue { get; }

	public float LookingAtDistance { get; }

	public int LookingAtMask { get; }

	public IResult Parse(string value)
	{
		return (_parser == null && !CommandArgumentParser.TryGetParser(Type, out _parser)) ? Result.Error("Missing argument parser!") : _parser.Parse(value, Type);
	}

	public CommandArgumentData(Type argType, string argName, bool optional, bool lookingAt, float lookingDistance, int lookingMask, object defaultValue, ValueRestrictionMode mode, object[] restrictedValues)
	{
		Type = argType;
		Name = argName;
		IsOptional = optional;
		IsLookingAt = lookingAt;
		LookingAtDistance = lookingDistance;
		LookingAtMask = lookingMask;
		DefaultValue = defaultValue;
		RestrictionMode = mode;
		RestrictedValues = restrictedValues;
		UserName = ArgumentUtils.GetFriendlyName(Type);
		if (!CommandArgumentParser.TryGetParser(argType, out var commandArgumentParser))
		{
			Log.Warning("Argument of type " + argType.FullName + " does not have a registered parser!", "Command Parser");
			_parser = null;
		}
		else
		{
			_parser = commandArgumentParser;
		}
	}
}
