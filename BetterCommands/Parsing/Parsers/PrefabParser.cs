using System;
using System.Collections.Generic;
using System.Linq;
using BetterCommands.Arguments.Prefabs;
using helpers.Results;
using Mirror;
using UnityEngine;

namespace BetterCommands.Parsing.Parsers;

public class PrefabParser : ICommandArgumentParser
{
	public IResult Parse(string value, Type type)
	{
		uint result;
		bool flag = uint.TryParse(value, out result);
		for (int i = 0; i < NetworkClient.prefabs.Count; i++)
		{
			KeyValuePair<uint, GameObject> keyValuePair = NetworkClient.prefabs.ElementAt(i);
			if (flag)
			{
				if (keyValuePair.Key == result)
				{
					return Result.Success(new PrefabData(i));
				}
			}
			else if (keyValuePair.Value.name == value)
			{
				return Result.Success(new PrefabData(i));
			}
		}
		return Result.Error("Failed to find a prefab by string " + value);
	}
}
