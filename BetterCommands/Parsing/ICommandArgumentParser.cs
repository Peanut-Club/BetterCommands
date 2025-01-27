using System;
using helpers.Results;

namespace BetterCommands.Parsing;

public interface ICommandArgumentParser
{
	IResult Parse(string value, Type type);
}
