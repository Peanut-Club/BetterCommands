using System;

namespace BetterCommands;

internal class ServerCommandSender : CommandSender
{
	public override string SenderId
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override string Nickname => "Server";

	public override ulong Permissions => 0uL;

	public override byte KickPower => 0;

	public override bool FullPermissions => true;

    public override bool Available() {
        throw new NotImplementedException();
    }

    public override void Print(string text)
	{
		throw new NotImplementedException();
	}

	public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
	{
		throw new NotImplementedException();
	}
}
