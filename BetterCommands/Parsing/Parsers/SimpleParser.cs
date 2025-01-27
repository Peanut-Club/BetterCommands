using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using helpers;
using helpers.Results;
using helpers.Time;

namespace BetterCommands.Parsing.Parsers;

public class SimpleParser : ICommandArgumentParser
{
	public static readonly SimpleParser Instance = new SimpleParser();

	public static void Register()
	{
		CommandArgumentParser.AddParser(Instance, typeof(string));
		CommandArgumentParser.AddParser(Instance, typeof(int));
		CommandArgumentParser.AddParser(Instance, typeof(uint));
		CommandArgumentParser.AddParser(Instance, typeof(byte));
		CommandArgumentParser.AddParser(Instance, typeof(sbyte));
		CommandArgumentParser.AddParser(Instance, typeof(short));
		CommandArgumentParser.AddParser(Instance, typeof(ushort));
		CommandArgumentParser.AddParser(Instance, typeof(long));
		CommandArgumentParser.AddParser(Instance, typeof(ulong));
		CommandArgumentParser.AddParser(Instance, typeof(float));
		CommandArgumentParser.AddParser(Instance, typeof(bool));
		CommandArgumentParser.AddParser(Instance, typeof(double));
		CommandArgumentParser.AddParser(Instance, typeof(Enum));
		CommandArgumentParser.AddParser(Instance, typeof(DateTime));
		CommandArgumentParser.AddParser(Instance, typeof(TimeSpan));
	}

	public IResult Parse(string value, Type type)
	{
		if (type == typeof(string))
		{
			return Result.Success(value.Trim());
		}
		int result;
		if (type == typeof(int))
		{
			return int.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result) ? Result.Success(result) : Result.Error("Failed to parse " + value + " to " + type.FullName);
		}
		uint result2;
		if (type == typeof(uint))
		{
			return uint.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result2) ? Result.Success(result2) : Result.Error("Failed to parse " + value + " to " + type.FullName);
		}
		byte result3;
		if (type == typeof(byte))
		{
			return byte.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result3) ? Result.Success(result3) : Result.Error("Failed to parse " + value + " to " + type.FullName);
		}
		sbyte result4;
		if (type == typeof(sbyte))
		{
			return sbyte.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result4) ? Result.Success(result4) : Result.Error("Failed to parse " + value + " to " + type.FullName);
		}
		short result5;
		if (type == typeof(short))
		{
			return short.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result5) ? Result.Success(result5) : Result.Error("Failed to parse " + value + " to " + type.FullName);
		}
		ushort result6;
		if (type == typeof(ushort))
		{
			return ushort.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result6) ? Result.Success(result6) : Result.Error("Failed to parse " + value + " to " + type.FullName);
		}
		long result7;
		if (type == typeof(long))
		{
			return long.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result7) ? Result.Success(result7) : Result.Error("Failed to parse " + value + " to " + type.FullName);
		}
		ulong result8;
		if (type == typeof(ulong))
		{
			return ulong.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result8) ? Result.Success(result8) : Result.Error("Failed to parse " + value + " to " + type.FullName);
		}
		float result9;
		if (type == typeof(float))
		{
			return float.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result9) ? Result.Success(result9) : Result.Error("Failed to parse " + value + " to " + type.FullName);
		}
		double result10;
		if (type == typeof(double))
		{
			return double.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result10) ? Result.Success(result10) : Result.Error("Failed to parse " + value + " to " + type.FullName);
		}
		bool result11;
		if (type == typeof(bool))
		{
			return bool.TryParse(value, out result11) ? Result.Success(result11) : Result.Error("Failed to parse " + value + " to " + type.FullName);
		}
		TimeSpan result12;
		if (type == typeof(TimeSpan))
		{
			return TimeUtils.TryParseTime(value, out result12) ? Result.Success(result12) : Result.Error("Failed to parse " + value + " to " + type.FullName);
		}
		DateTime result13;
		if (type == typeof(DateTime))
		{
			return DateTime.TryParse(value, out result13) ? Result.Success(result13) : Result.Error("Failed to parse " + value + " to " + type.FullName);
		}
		if (type.IsEnum)
		{
			try
			{
				IEnumerable<Enum> values = Enum.GetValues(type).Cast<Enum>();
				if (!int.TryParse(value, out var enumIndex))
				{
					object obj = Enum.Parse(type, value, ignoreCase: true);
					if (obj != null)
					{
						return Result.Success(obj);
					}
					return Result.Error("Failed to parse enum: " + value + " (" + type.FullName + ")");
				}
				if (values.TryGetFirst((Enum enumValue) => string.Equals(Convert.ChangeType(enumValue, enumValue.GetTypeCode()).ToString(), enumIndex.ToString(), StringComparison.InvariantCulture), out var value2))
				{
					return Result.Success(value2);
				}
			}
			catch (Exception arg)
			{
				return Result.Error($"Failed to parse {value} to {type.Name}: \n{arg}");
			}
		}
		return Result.Error("An unsupported type was provided to the simple type parser: " + type.FullName);
	}
}
