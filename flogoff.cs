using System;
using System.ComponentModel;
using System.Collections.Generic;
using TShockAPI;
using Terraria;

namespace Flogoff
{
    [APIVersion(1, 11)]

    public class FLogoff : TerrariaPlugin
    {
        private List<string> offline = new List<string>();

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
            Order = -1;
        }

        public override void Initialize()
        {
            Hooks.ServerHooks.Chat += OnChat;
            Commands.ChatCommands.Add(new Command("flogoff", flogon, "flogon"));
            Commands.ChatCommands.Add(new Command("flogoff", flogoff, "flogoff"));
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Hooks.ServerHooks.Chat -= OnChat;
            }
            base.Dispose(disposing);
        }


        private void OnChat(messageBuffer msg, int who, string message, HandledEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            TSPlayer player = TShock.Players[msg.whoAmI];

            if (player == null)
            {
                args.Handled = true;
                return;
            }

            if (message.StartsWith("/"))
            {
                if (message.Substring(1, 3) == "tp")
                {

                    string[] words = message.Split();
                    string cmd = words[0].Substring(1);
                    string tpto = words[1];

                    string result = offline.Find(delegate(string off) { return off == tpto; });

                    if (result == null)
                    {
                        args.Handled = false;
                        return;
                    }
                    else
                    {
                        args.Handled = true;
                        player.SendMessage("Invalid player!", Color.Red);
                        return;
                    }
                }
                else
                {
                    args.Handled = false;
                    return;
                }
            }
        }

        private void flogoff(CommandArgs args)
        {
            TSPlayer player = args.Player;
            player.mute = true; //Just for saftey ;)
            offline.Add(player.Name);
            player.SetBuff(10,72000,true);
            TSPlayer.All.SendMessage(string.Format("{0} left", player.Name), Color.Yellow);
        }

        private void flogon(CommandArgs args)
        {
            TSPlayer player = args.Player;
            player.mute = false;
            offline.Remove(player.Name);
            player.SetBuff(0, 0, true);
            TSPlayer.All.SendMessage(string.Format("{0} has joined", player.Name), Color.Yellow);
        }
    }
}
