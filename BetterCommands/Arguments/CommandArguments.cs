using System.Collections.Generic;
using BetterCommands.Parsing;
using helpers.Extensions;
using helpers.Results;

namespace BetterCommands.Arguments;

public class CommandArguments
{
	private readonly Dictionary<string, string> m_Args = new Dictionary<string, string>();

	public string GetString(string key)
	{
		string value;
		return (!TryGetString(key, out value)) ? null : value;
	}

	public TValue GetValue<TValue>(string key)
	{
		TValue value;
		return (!TryGetValue<TValue>(key, out value)) ? default(TValue) : value;
	}

	public bool TryGetString(string key, out string value)
	{
		return m_Args.TryGetValue(key, out value);
	}

	public bool TryGetValue<TValue>(string key, out TValue value)
	{
		if (!TryGetString(key, out var value2))
		{
			value = default(TValue);
			return false;
		}
		if (!CommandArgumentParser.TryGetParser(typeof(TValue), out var commandArgumentParser) || commandArgumentParser == null)
		{
			value = default(TValue);
			return false;
		}
		IResult result = commandArgumentParser.Parse(value2, typeof(TValue));
		if (!result.IsSuccess)
		{
			value = default(TValue);
			return false;
		}
		return result.TryReadResult<TValue>(failOnError: true, out value);
	}

	public void Parse(string value)
	{
		m_Args.Clear();
		if (value == null || !value.TryParse(out var parts))
		{
			return;
		}
		string[] array = parts;
		string[] array2 = array;
		foreach (string text in array2)
		{
			string text2 = text;
			if (text2.StartsWith("-"))
			{
				text2 = text2.Remove("-");
			}
			if (text2.TrySplit('=', removeEmptyOrWhitespace: true, 2, out var splits))
			{
				m_Args[splits[0].Trim()] = splits[1].Trim();
			}
		}
	}
}
