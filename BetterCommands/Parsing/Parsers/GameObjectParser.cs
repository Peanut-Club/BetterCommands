using System;
using System.Collections.Generic;
using helpers;
using helpers.Results;
using UnityEngine;

namespace BetterCommands.Parsing.Parsers;

public class GameObjectParser : ICommandArgumentParser
{
	public static IReadOnlyList<GameObject> AllObjects
	{
		get
		{
			List<GameObject> list = new List<GameObject>();
			list.AddRange(UnityEngine.Object.FindObjectsOfType<GameObject>());
			return list;
		}
	}

	public IResult Parse(string value, Type type)
	{
		IReadOnlyList<GameObject> allObjects = AllObjects;
		int id;
		bool parsed = int.TryParse(value, out id);
		GameObject value2;
		return allObjects.TryGetFirst((GameObject obj) => obj.name == value || (parsed && obj.GetInstanceID() == id), out value2) ? Result.Success(value2) : Result.Error("Failed to find a game object by string " + value);
	}
}
