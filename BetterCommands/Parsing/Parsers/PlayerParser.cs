using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using helpers;
using helpers.Extensions;
using helpers.Results;
using PluginAPI.Core;
using PluginAPI.Core.Interfaces;

namespace BetterCommands.Parsing.Parsers;

public class PlayerParser : ICommandArgumentParser
{
	public IResult Parse(string value, Type type)
	{
		Player value2 = null;
		IEnumerable<Player> enumerable = from player in Player.GetPlayers()
			where !player.IsServer
			select player;
		if (int.TryParse(value, out var playerId) && enumerable.TryGetFirst((Player player) => player.PlayerId == playerId, out value2))
		{
			return ToResult(value2);
		}
		if (value.StartsWith("nid"))
		{
			value = value.Remove("nid", "nid:");
			if (uint.TryParse(value, out var netId) && enumerable.TryGetFirst((Player player) => player.NetworkId == netId, out value2))
			{
				return ToResult(value2);
			}
		}
		if ((IPAddress.TryParse(value, out var ip) && enumerable.TryGetFirst((Player player) => player.IpAddress == ip.ToString(), out value2)) || enumerable.TryGetFirst((Player player) => player.UserId == value || player.UserId.StartsWith(value), out value2))
		{
			return ToResult(value2);
		}
		value2 = (from player in enumerable
			where player.Nickname.GetSimilarity(value) > 0.0
			orderby player.Nickname.GetSimilarity(value) descending
			select player).FirstOrDefault();
		return (value2 == null) ? Result.Error("Failed to find a player by string: " + value) : ToResult(value2);
		IResult ToResult(Player player)
		{
			if (player == null)
			{
				return Result.Error("Failed to find a target player by string: " + value);
			}
			if (type == typeof(Player) || type == typeof(IPlayer))
			{
				return Result.Success(player);
			}
			if (!FactoryManager.FactoryTypes.TryGetValue(type, out var value3))
			{
				return Result.Error("Failed to find a player factory for player type: " + type.FullName);
			}
			if (!FactoryManager.PlayerFactories.TryGetValue(value3, out var value4))
			{
				return Result.Error("Failed to find a player factory by type: " + value3.FullName);
			}
			Player orAdd = value4.GetOrAdd((IGameComponent)(object)player.ReferenceHub);
			return (orAdd == null) ? Result.Error("Failed to fetch a player from factory: " + value3.FullName) : Result.Success(orAdd);
		}
	}
}
