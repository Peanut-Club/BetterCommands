using System;
using CentralAuth;
using helpers.Results;
using PlayerRoles;
using PlayerStatsSystem;

namespace BetterCommands.Conditions;

public struct ConditionData
{
	public ConditionFlag Flags { get; }

	public object ConditionObject { get; }

	public ConditionData(ConditionFlag flags, object condObject)
	{
		Flags = flags;
		ConditionObject = condObject;
	}

	public IResult Validate(ReferenceHub hub)
	{
		if (Flags.HasFlag(ConditionFlag.RoleTypeOnly))
		{
			if (ConditionObject is RoleTypeId roleTypeId)
			{
				return (hub.GetRoleId() != roleTypeId) ? Result.Error($"Condition failed: You must be a {roleTypeId} to run this command.") : Result.Success();
			}
			return Result.Error("Condition failed: The condition has a RoleTypeOnly flag, but the condition's object is not a role type!");
		}
		if (Flags.HasFlag(ConditionFlag.DisableServerPlayer))
		{
			return (hub.Mode != ClientInstanceMode.ReadyClient) ? Result.Error("Condition failed: You cannot run this command as the server.") : Result.Success();
		}
		if (Flags.HasFlag(ConditionFlag.HealthOnly))
		{
			if (ConditionObject is float num)
			{
				return ((double)hub.playerStats.GetModule<HealthStat>().NormalizedValue != (double)num) ? Result.Error($"Condition failed: You must have precisely {num} health to run this command.") : Result.Success();
			}
			return Result.Error("Condition failed: The condition has a HealthOnly flag, but the condition's object is not a valid floating-point number!");
		}
		if (!Flags.HasFlag(ConditionFlag.Custom))
		{
			return Result.Success();
		}
		return (!(ConditionObject is Func<ReferenceHub, IResult> func)) ? Result.Error("Condition failed: The condition has a Custom flag, but the condition's object is not a valid delegate!") : func(hub);
	}
}
