﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Terraria;
using TShockAPI;

namespace Flogoff
{
    [APIVersion(1, 11)]

    public class FLogoff : TerrariaPlugin
    {
        protected List<string> offline = new List<string>();

        public override Version Version
        {
            get { return new Version("1.0.8"); }
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
            /* There is an error here somewhere...
            bool perm = false;

            foreach (Group group in TShock.Groups.groups)
            {
                if (group.Name != "superadmin")
                {
                    if (group.HasPermission("flogoff"))
                        perm = true;
                }
            }

            List<string> permlist = new List<string>();
            if (!perm)
                permlist.Add("flogoff");
            TShock.Groups.AddPermissions("admin", permlist);*/
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Hooks.ServerHooks.Chat -= OnChat;
            }
            base.Dispose(disposing);
        }


        protected void OnChat(messageBuffer msg, int who, string message, HandledEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            TSPlayer player = TShock.Players[msg.whoAmI];

            if (message.Substring(0, 3) != "/tp" && message.Substring(0, 7) != "/playing")
            {
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
                        return;
                    }
                    else
                    {
                        args.Handled = true;
                        player.SendMessage("Invalid player!", Color.Red);
                        return;
                    }
                }
                if(message.Substring(1,7)=="playing")
                {
                    args.Handled = true;
                    string response = TShock.Utils.GetPlayers();
                    string[] players = response.Split();
                    int i = 0;
                    foreach(string playername in players){
                        string result = offline.Find(delegate(string off) { return off == playername; });
                        if (result != null)
                        {
                            players[i] = "";
                        }
                        i++;
                    }
                    response = String.Join(" ",players);
                    player.SendMessage(string.Format("Current players: {0}.", response), 255, 240, 20);
                }
                
                return;
            }
        }

        protected void flogoff(CommandArgs args)
        {
            TSPlayer player = args.Player;
            player.mute = true; //Just for saftey ;)
            offline.Add(player.Name);
            player.SetBuff(10,72000,true);
            TSPlayer.All.SendMessage(string.Format("{0} left", player.Name), Color.Yellow);
        }

        protected void flogon(CommandArgs args)
        {
            TSPlayer player = args.Player;
            player.mute = false;
            offline.Remove(player.Name);
            TSPlayer.All.SendMessage(string.Format("{0} has joined", player.Name), Color.Yellow);
        }
    }
}
