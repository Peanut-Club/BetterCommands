using System;
using helpers.Results;
using Interactables.Interobjects.DoorUtils;

namespace BetterCommands.Parsing.Parsers;

public class DoorParser : ICommandArgumentParser
{
	public IResult Parse(string value, Type type)
	{
		foreach (DoorNametagExtension value2 in DoorNametagExtension.NamedDoors.Values)
		{
			if (!string.IsNullOrWhiteSpace(value2.GetName) && value2.TargetDoor != null)
			{
				if (string.Equals(value, value2.GetName, StringComparison.OrdinalIgnoreCase))
				{
					return Result.Success(value2.TargetDoor);
				}
				if (int.TryParse(value, out var result) && (value2.TargetDoor.netId == result || value2.TargetDoor.GetInstanceID() == result))
				{
					return Result.Success(value2.TargetDoor);
				}
			}
		}
		return Result.Error("Failed to find a door by " + value);
	}
}
