using System;

namespace BetterCommands.Conditions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class ConditionAttribute : Attribute
{
	public ConditionFlag Flags { get; }

	public object ConditionObject { get; }

	public ConditionAttribute(object conditionObject, params ConditionFlag[] flags)
	{
		ConditionObject = conditionObject;
		Flags = ConditionFlag.DisableServerPlayer;
		foreach (ConditionFlag conditionFlag in flags)
		{
			Flags &= conditionFlag;
		}
	}

	public ConditionAttribute(params ConditionFlag[] flags)
	{
		Flags = ConditionFlag.DisableServerPlayer;
		foreach (ConditionFlag conditionFlag in flags)
		{
			Flags &= conditionFlag;
		}
	}
}
