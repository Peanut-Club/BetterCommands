using System;
using System.Collections;
using HarmonyLib;
using helpers;
using helpers.Results;

namespace BetterCommands.Parsing.Parsers;

public class CollectionParser : ICommandArgumentParser
{
	public static readonly CollectionParser Instance = new CollectionParser();

	public static void Register()
	{
		CommandArgumentParser.AddParser(Instance, typeof(Array));
		CommandArgumentParser.AddParser(Instance, typeof(IDictionary));
		CommandArgumentParser.AddParser(Instance, typeof(IEnumerable));
	}

	public IResult Parse(string value, Type type)
	{
		Type[] genericArguments = type.GetGenericArguments();
		if (type.IsArray)
		{
			string[] array = value.Split(new char[1] { ',' });
			if (!CommandArgumentParser.TryGetParser(genericArguments[0], out var commandArgumentParser))
			{
				return Result.Error("Failed to retrieve a parser for array element type: " + genericArguments[0].FullName);
			}
			Array array2 = Array.CreateInstance(genericArguments[0], array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				IResult result = commandArgumentParser.Parse(array[i], genericArguments[0]);
				if (!result.IsSuccess)
				{
					return Result.Error($"Failed to parse argument at index {i}: {result.ReadError()}");
				}
				array2.SetValue(result.ReadResult<object>(), i);
			}
			return Result.Success(array2);
		}
		object obj = Reflection.Instantiate(type);
		object obj2 = obj;
		if (!(obj2 is IDictionary dictionary))
		{
			if (obj2 is IList list)
			{
				string[] array3 = value.Split(new char[1] { ',' });
				if (!CommandArgumentParser.TryGetParser(genericArguments[0], out var commandArgumentParser2))
				{
					return Result.Error("Failed to find a parser for list element type: " + genericArguments[0].FullName);
				}
				for (int j = 0; j < array3.Length; j++)
				{
					IResult result2 = commandArgumentParser2.Parse(array3[j], genericArguments[0]);
					if (!result2.IsSuccess)
					{
						return Result.Error($"Failed to parse element at index {j}: {result2.ReadError()}");
					}
					list.Add(result2.ReadResult<object>());
				}
				return Result.Success(list);
			}
			return Result.Error("Failed to parse collection: unsupported! " + type.FullDescription());
		}
		string[] array4 = value.Split(new char[1] { ';' });
		Type type2 = genericArguments[0];
		Type type3 = genericArguments[1];
		if (!CommandArgumentParser.TryGetParser(type2, out var commandArgumentParser3))
		{
			return Result.Error("Failed to retrieve a parser for dictionary key: " + type2.FullName);
		}
		if (!CommandArgumentParser.TryGetParser(type3, out var commandArgumentParser4))
		{
			return Result.Error("Failed to retrieve a parser for dictionary value: " + type3.FullName);
		}
		for (int k = 0; k < array4.Length; k++)
		{
			string text = array4[k];
			string[] array5 = text.Split(new char[1] { ':' });
			if (array5.Length != 2)
			{
				return Result.Error($"Failed to split {text} into a pair! (index: {k})");
			}
			string value2 = array5[0];
			string value3 = array5[1];
			IResult result3 = commandArgumentParser3.Parse(value2, type2);
			if (!result3.IsSuccess)
			{
				return Result.Error($"Failed to parse key at index {k}: {result3.ReadError()}");
			}
			IResult result4 = commandArgumentParser4.Parse(value3, type3);
			if (!result4.IsSuccess)
			{
				return Result.Error($"Failed to parse value at index {k}: {result4.ReadError()}");
			}
			dictionary[result3.ReadResult<object>()] = result4.ReadResult<object>();
		}
		return Result.Success(dictionary);
	}
}
