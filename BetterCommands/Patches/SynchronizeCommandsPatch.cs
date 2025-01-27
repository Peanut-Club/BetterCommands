using System.Collections.Generic;
using BetterCommands.Management;
using CommandSystem;
using HarmonyLib;
using RemoteAdmin;

namespace BetterCommands.Patches;

[HarmonyPatch(typeof(QueryProcessor), "ParseCommandsToStruct")]
public static class SynchronizeCommandsPatch
{
	public static bool Prefix(List<ICommand> list, ref QueryProcessor.CommandData[] __result)
	{
		List<QueryProcessor.CommandData> dataList = new List<QueryProcessor.CommandData>();
		list.ForEach(delegate(ICommand x)
		{
			string text = x.Description;
			if (string.IsNullOrWhiteSpace(text))
			{
				text = null;
			}
			else if (text.Length > 80)
			{
				text = text.Substring(0, 80) + "...";
			}
			QueryProcessor.CommandData data = default(QueryProcessor.CommandData);
			data.Command = x.Command;
			data.Description = text;
			data.Usage = ((!(x is IUsageProvider usageProvider)) ? null : usageProvider.Usage);
			data.AliasOf = null;
			data.Hidden = x is IHiddenCommand;
			dataList.Add(data);
			if (x.Aliases != null && x.Aliases.Length != 0)
			{
				x.Aliases.ForEach(delegate(string y)
				{
					dataList.Add(new QueryProcessor.CommandData
					{
						Command = y,
						Usage = null,
						Description = null,
						AliasOf = data.Command,
						Hidden = data.Hidden
					});
				});
			}
		});
		CommandManager.Synchronize(dataList);
		__result = dataList.ToArray();
		return false;
	}
}
