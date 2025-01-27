using System;
using System.Collections.Generic;
using BetterCommands.Arguments.Effects;
using CustomPlayerEffects;
using helpers;
using helpers.Results;
using PluginAPI.Loader;

namespace BetterCommands.Parsing.Parsers;

public class EffectParser : ICommandArgumentParser
{
	private static readonly List<Type> m_Effects = new List<Type>();

	public static IReadOnlyList<Type> EffectTypes
	{
		get
		{
			if (!m_Effects.Any())
			{
				Type[] types = AssemblyLoader.MainAssembly.GetTypes();
				Type[] array = types;
				foreach (Type type in array)
				{
					if (!(type.Namespace != "CustomPlayerEffects") && Reflection.HasType<StatusEffectBase>(type))
					{
						m_Effects.Add(type);
					}
				}
			}
			return m_Effects;
		}
	}

	public IResult Parse(string value, Type type)
	{
		Type value2;
		return (!EffectTypes.TryGetFirst((Type effect) => string.Equals(effect.Name, value, StringComparison.OrdinalIgnoreCase), out value2)) ? Result.Error("Failed to find an effect's type: " + value) : Result.Success(new EffectData(value2, value2.Name));
	}
}
