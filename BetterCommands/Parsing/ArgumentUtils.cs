using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdminToys;
using BetterCommands.Arguments.Effects;
using BetterCommands.Arguments.Prefabs;
using BetterCommands.Arguments.Toys;
using BetterCommands.Parsing.Parsers;
using helpers;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using Mirror;
using PluginAPI.Core.Interfaces;
using UnityEngine;

namespace BetterCommands.Parsing;

public static class ArgumentUtils
{
	private static readonly Dictionary<Type, string> _userFriendlyNames;

	public static IReadOnlyDictionary<Type, string> UserFriendlyNames => _userFriendlyNames;

	public static void SetFriendlyName(Type type, string str)
	{
		_userFriendlyNames[type] = str;
	}

	public static string GetFriendlyName(Type type)
	{
		Type type2 = type;
		if (Reflection.HasInterface<IPlayer>(type))
		{
			type = typeof(IPlayer);
		}
		if (Reflection.HasInterface<IDictionary>(type))
		{
			type = typeof(IDictionary);
		}
		if (type.IsArray)
		{
			type = typeof(Array);
		}
		if (Reflection.HasInterface<IEnumerable>(type) && type != typeof(IDictionary) && type != typeof(string) && type != typeof(Array))
		{
			type = typeof(IEnumerable);
		}
		if (type.IsEnum)
		{
			type = typeof(Enum);
		}
		if (UserFriendlyNames.TryGetValue(type, out var value))
		{
			ReplaceValues(ref value, type, type2);
			return value;
		}
		return type2.Name;
	}

	public static void ReplaceValues(ref string str, Type type, Type origType)
	{
		Type[] genericArguments = origType.GetGenericArguments();
		if (type == typeof(Enum))
		{
			Array values = Enum.GetValues(origType);
			object[] array = new object[values.Length];
			values.CopyTo(array, 0);
			str = str.Replace("%values%", string.Join(", ", array.Select((object x) => x.ToString())));
			if (str.Length >= 50)
			{
				str = str.Substring(0, 49) + ".. for a full list of available values, use this command: \"values " + origType.FullName + "\"";
			}
		}
		else if (type == typeof(IDictionary))
		{
			str = str.Replace("%keyType%", GetFriendlyName(genericArguments[0])).Replace("%valueType%", GetFriendlyName(genericArguments[1]));
		}
		else if (type == typeof(Array))
		{
			str = str.Replace("%valueType%", GetFriendlyName(origType.GetElementType()));
		}
		else if (type == typeof(IEnumerable))
		{
			str = str.Replace("%valueType%", GetFriendlyName(genericArguments[0]));
		}
		else if (type == typeof(ToyAllowedNewArgumentData) || type == typeof(ToyDisallowedNewArgumentData))
		{
			AdminToyBase component;
			List<GameObject> source = NetworkClient.prefabs.Where(addNull: false, (GameObject prefab) => prefab.TryGetComponent<AdminToyBase>(out component));
			str = str.Replace("%values%", string.Join(", ", source.Select((GameObject toy) => toy.GetComponent<AdminToyBase>().CommandName)));
		}
		else if (type == typeof(EffectData))
		{
			str = str.Replace("%values%", string.Join(", ", EffectParser.EffectTypes.Select((Type eType) => eType.Name)));
		}
	}

	public static bool TryGetLookingAt(ReferenceHub sender, float distance, int mask, Type type, out object hit)
	{
		RaycastHit[] array = Physics.RaycastAll(new Ray(sender.transform.position, sender.transform.forward), distance, mask, QueryTriggerInteraction.Ignore);
		if (!array.Any())
		{
			hit = null;
			return false;
		}
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			if (!(raycastHit.transform != null))
			{
				continue;
			}
			Transform transform = raycastHit.transform.parent ?? raycastHit.transform;
			if (!(transform.gameObject == sender.gameObject))
			{
				if (type == typeof(GameObject))
				{
					hit = transform.gameObject;
					return true;
				}
				if (type == typeof(Transform))
				{
					hit = transform;
					return true;
				}
				if (transform.TryGetComponent(type, out var component))
				{
					hit = component;
					return true;
				}
				component = transform.GetComponentInParent(type);
				if (component != null)
				{
					hit = component;
					return true;
				}
				component = transform.GetComponentInChildren(type);
				if (component != null)
				{
					hit = component;
					return true;
				}
			}
		}
		hit = null;
		return false;
	}

	static ArgumentUtils()
	{
		_userFriendlyNames = new Dictionary<Type, string>
		{
			[typeof(DoorVariant)] = "a door's name or ID",
			[typeof(Array)] = "a list of values [%valueType%]",
			[typeof(ReferenceHub)] = "player's nickname, user ID, player ID or IP address",
			[typeof(Enum)] = "an enum [possible values: %values%]",
			[typeof(RoomIdentifier)] = "a room's name or ID",
			[typeof(GameObject)] = "a game object's name or ID",
			[typeof(NetworkIdentity)] = "an ID of a network identity",
			[typeof(ToyAllowedNewArgumentData)] = "an admin toy's network ID or name [possible names: %values%]",
			[typeof(ToyDisallowedNewArgumentData)] = "an admin toy's network ID or name [possible names: %values%]",
			[typeof(EffectData)] = "an effect's name [possible names: %values%]",
			[typeof(PrefabData)] = "a name of a prefab [possible names: %values%]",
			[typeof(TimeSpan)] = "a timestamp, for example 00:00:00",
			[typeof(DateTime)] = "a date, for example 5/1/2023 15:00:00",
			[typeof(IPlayer)] = "player's nickname, user ID, player ID or IP address",
			[typeof(IDictionary)] = "a list of paired values [key: %keyType% | value: %valueType%]",
			[typeof(IEnumerable)] = "a list of values [%valueType%]",
			[typeof(string)] = "a string value [word(s)]",
			[typeof(bool)] = "a boolean value [true or false]",
			[typeof(int)] = $"a number between {int.MinValue} and {int.MaxValue}",
			[typeof(uint)] = $"a number between {0u} and {uint.MaxValue}",
			[typeof(byte)] = $"a number between {(byte)0} and {byte.MaxValue}",
			[typeof(sbyte)] = $"a number between {sbyte.MinValue} and {sbyte.MaxValue}",
			[typeof(short)] = $"a number between {short.MinValue} and {short.MaxValue}",
			[typeof(ushort)] = $"a number between {(ushort)0} and {ushort.MaxValue}",
			[typeof(long)] = $"a number between {long.MinValue} and {long.MaxValue}",
			[typeof(ulong)] = $"a number between {0uL} and {ulong.MaxValue}",
			[typeof(float)] = $"a floating-point number between {float.MinValue} and {float.MaxValue}",
			[typeof(double)] = $"a number between {double.MinValue} and {double.MaxValue}"
		};
	}
}
