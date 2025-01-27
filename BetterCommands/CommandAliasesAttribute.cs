using System;
using System.Linq;

namespace BetterCommands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CommandAliasesAttribute : Attribute
{
	public string[] Aliases { get; }

	public CommandAliasesAttribute(params object[] aliases)
	{
		string[] array = aliases?.Select((object x) => x?.ToString() ?? null)?.ToArray();
		if (array == null)
		{
			array = Array.Empty<string>();
		}
		Aliases = array;
	}
}
