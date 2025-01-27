using System;
using System.Collections.Generic;
using helpers;
using helpers.Results;
using MapGeneration;

namespace BetterCommands.Parsing.Parsers;

public class RoomIdentifierParser : ICommandArgumentParser
{
	public IResult Parse(string value, Type type)
	{
		HashSet<RoomIdentifier> allRoomIdentifiers = RoomIdentifier.AllRoomIdentifiers;
		RoomName roomName;
		bool parsedName = Enum.TryParse<RoomName>(value, ignoreCase: true, out roomName);
		RoomIdentifier value2;
		return allRoomIdentifiers.TryGetFirst((RoomIdentifier room) => (parsedName && room.Name == roomName) || room.name == value, out value2) ? Result.Success(value2) : Result.Error("Failed to find a room by string: " + value);
	}
}
