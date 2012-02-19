using System;
using TShockAPI;
using Terraria;

namespace Flogoff
{
    [APIVersion(1, 11)]

    public class FLogoff : TerrariaPlugin
    {
        public override Version Version
        {
            get { return new Version("1.0.2"); }
        }

        public override string Name
        {
            get { return "FLogoff"; }
        }

        public override string Author
        {
            get { return "Darkvengance aka Sildaekar"; }
        }

        public override string Description
        {
            get { return "Performs a fake logoff"; }
        }

        public FLogoff(Main game)
            : base(game)
        {
            Order = 4;
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("kick", flogoff, "flogoff"));
        }

        private void flogoff(CommandArgs args)
        {
            TSPlayer player = args.Player;
            player.TPAllow = false; //Until we find something better
            player.mute = true; //Just for saftey ;)
            player.SetBuff(10,72000,true);
            Log.Info(string.Format("{0} did a fake logoff.", player.Name));
            TSPlayer.All.SendMessage(string.Format("{0} left", player.Name), Color.Yellow);
        }
    }
}
