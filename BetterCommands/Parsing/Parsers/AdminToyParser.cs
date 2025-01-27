using System;
using System.Linq;
using AdminToys;
using BetterCommands.Arguments.Toys;
using helpers;
using helpers.Results;
using Mirror;
using UnityEngine;

namespace BetterCommands.Parsing.Parsers;

public class AdminToyParser : ICommandArgumentParser
{
	public static readonly AdminToyParser Instance = new AdminToyParser();

	public static void Register()
	{
		CommandArgumentParser.AddParser(Instance, typeof(ToyAllowedNewArgumentData));
		CommandArgumentParser.AddParser(Instance, typeof(ToyDisallowedNewArgumentData));
	}

	public IResult Parse(string value, Type type)
	{
		bool isNewAllowed = type == typeof(ToyAllowedNewArgumentData);
		AdminToyBase[] array = UnityEngine.Object.FindObjectsOfType<AdminToyBase>();
		if (array.Any())
		{
			AdminToyBase value3;
			if (uint.TryParse(value, out var netId))
			{
				if (array.TryGetFirst((AdminToyBase toy) => toy.netId == netId || toy.GetInstanceID() == (int)netId, out var value2))
				{
					return ReturnCorrect(isNew: false, value2);
				}
			}
			else if (array.TryGetFirst((AdminToyBase toy) => toy.CommandName == value, out value3))
			{
				return ReturnCorrect(isNew: false, value3);
			}
		}
		if (!NetworkClient.prefabs.Values.TryGetFirst((GameObject toyPrefab) => toyPrefab.TryGetComponent<AdminToyBase>(out var component2) && component2.CommandName == value, out var value4))
		{
			return Result.Error("Failed to find a toy prefab for " + value);
		}
		AdminToyBase component;
		return (!UnityEngine.Object.Instantiate(value4).TryGetComponent<AdminToyBase>(out component)) ? Result.Error("Failed to retrieve the toy component from a new instance!") : ReturnCorrect(isNew: true, component);
		IResult ReturnCorrect(bool isNew, AdminToyBase toy)
		{
			return isNewAllowed ? Result.Success(new ToyAllowedNewArgumentData(isNew, toy)) : Result.Success(new ToyDisallowedNewArgumentData(toy));
		}
	}
}
