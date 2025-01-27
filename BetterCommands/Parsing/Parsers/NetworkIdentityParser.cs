using System;
using helpers;
using helpers.Results;
using Mirror;

namespace BetterCommands.Parsing.Parsers;

public class NetworkIdentityParser : ICommandArgumentParser
{
	public IResult Parse(string value, Type type)
	{
		if (!uint.TryParse(value, out var netId))
		{
			return Result.Error("Failed to parse network ID!");
		}
		NetworkIdentity value2;
		return (!NetworkClient.spawned.Values.TryGetFirst((NetworkIdentity identity) => identity.netId == netId, out value2)) ? Result.Error($"Failed to find a spawned network identity with ID {netId}") : Result.Success(value2);
	}
}
