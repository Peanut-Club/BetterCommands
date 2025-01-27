using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CentralAuth;
using Compendium;
using helpers;
using helpers.Extensions;
using helpers.Results;

namespace BetterCommands.Parsing.Parsers;

public class ReferenceHubParser : ICommandArgumentParser
{
	public IResult Parse(string value, Type type)
	{
		ReferenceHub value2 = null;
		IEnumerable<ReferenceHub> enumerable = ReferenceHub.AllHubs.Where((ReferenceHub player) => player.Mode == ClientInstanceMode.ReadyClient && !string.IsNullOrWhiteSpace(player.UserId()) && !string.IsNullOrWhiteSpace(player.nicknameSync.Network_myNickSync) && player.connectionToClient != null);
		if (int.TryParse(value, out var playerId) && enumerable.TryGetFirst((ReferenceHub player) => player.PlayerId == playerId, out value2))
		{
			return Result.Success(value2);
		}
		if (value.StartsWith("nid"))
		{
			value = value.Remove("nid", "nid:");
			if (uint.TryParse(value, out var netId) && enumerable.TryGetFirst((ReferenceHub player) => player.netId == netId, out value2))
			{
				return Result.Success(value2);
			}
		}
		if (IPAddress.TryParse(value, out var ip) && enumerable.TryGetFirst((ReferenceHub player) => player.connectionToClient.address == ip.ToString(), out value2))
		{
			return Result.Success(value2);
		}
		if (enumerable.TryGetFirst((ReferenceHub player) => player.UserId() == value || player.UserId().StartsWith(value), out value2))
		{
			return Result.Success(value2);
		}
		value2 = (from player in enumerable
			where player.nicknameSync.Network_myNickSync.GetSimilarity(value) > 0.0
			orderby player.nicknameSync.Network_myNickSync.GetSimilarity(value) descending
			select player).FirstOrDefault();
		return ((object)value2 == null) ? Result.Error("Failed to find a player by string: " + value) : Result.Success(value2);
	}
}
