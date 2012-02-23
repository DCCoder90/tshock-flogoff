﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Terraria;
using TShockAPI;
using Hooks;

namespace Flogoff
{
    [APIVersion(1, 11)]

    public class FLogoff : TerrariaPlugin
    {
        protected List<string> offline = new List<string>();
        protected static bool[] offlineindex = new bool[256];

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
            NetHooks.SendData += OnSendData;
            Commands.ChatCommands.Add(new Command("flogoff", flogon, "flogon"));
            Commands.ChatCommands.Add(new Command("flogoff", flogoff, "flogoff"));
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Hooks.ServerHooks.Chat -= OnChat;
                NetHooks.SendData -= OnSendData;
            }
            base.Dispose(disposing);
        }

        public void OnSendData(SendDataEventArgs e)
        {
            try
            {
                List<int> list = new List<int>();
                for (int i = 0; i < 256; i++)
                {
                    if (FLogoff.offlineindex[i])
                    {
                        list.Add(i);
                    }
                }
                PacketTypes msgID = e.MsgID;
                if (msgID <= PacketTypes.DoorUse)
                {
                    if (msgID != PacketTypes.PlayerSpawn && msgID != PacketTypes.DoorUse)
                    {
                        goto IL_D2;
                    }
                }
                else
                {
                    switch (msgID)
                    {
                        case PacketTypes.PlayerDamage:
                            break;

                        case PacketTypes.ProjectileNew:
                        case PacketTypes.ProjectileDestroy:
                            if (list.Contains(e.ignoreClient) && FLogoff.offlineindex[e.ignoreClient])
                            {
                                e.Handled = true;
                                goto IL_D2;
                            }
                            goto IL_D2;

                        case PacketTypes.NpcStrike:
                            goto IL_D2;

                        default:
                            switch (msgID)
                            {
                                case PacketTypes.EffectHeal:
                                case PacketTypes.Zones:
                                    break;

                                default:
                                    switch (msgID)
                                    {
                                        case PacketTypes.PlayerAnimation:
                                        case PacketTypes.EffectMana:
                                        case PacketTypes.PlayerTeam:
                                            break;

                                        case PacketTypes.PlayerMana:
                                        case PacketTypes.PlayerKillMe:
                                            goto IL_D2;

                                        default:
                                            goto IL_D2;
                                    }
                                    break;
                            }
                            break;
                    }
                }
                if (list.Contains(e.number) && FLogoff.offlineindex[e.number])
                {
                    e.Handled = true;
                }
            IL_D2:
                if (e.number >= 0 && e.number <= 255 && FLogoff.offlineindex[e.number] && e.MsgID == PacketTypes.PlayerUpdate)
                {
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
            }
        }

        protected void OnChat(messageBuffer msg, int who, string message, HandledEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            TSPlayer player = TShock.Players[msg.whoAmI];

            if (message.StartsWith("/tp") || message.StartsWith("/playing"))
            {
                if (message.Substring(1, 3) == "tp")
                {

                    string[] words = message.Split();
                    string cmd = words[0].Substring(1);
                    string tpto = words[1];

                    string result = offline.Find(delegate(string off) {return off == tpto; });

                    if (result != null&&result!="")
                    {
                        args.Handled = true;
                        player.SendMessage("Invalid player!", Color.Red);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                if (message.Substring(1, 7) == "playing")
                {
                    args.Handled = true;
                    string response = TShock.Utils.GetPlayers();
                    string[] players = response.Split();
                    int i = 0;
                    foreach (string playername in players)
                    {
                        string result = offline.Find(delegate(string off) { return off.ToLower() == playername.ToLower(); });
                        if (result != null)
                        {
                            players[i] = "";
                        }
                        i++;
                    }
                    response = String.Join(" ", players);
                    player.SendMessage(string.Format("Current players: {0}.", response), 255, 240, 20);
                }
            }
            else
            {
                return;
            }
        }

        protected void flogoff(CommandArgs args)
        {
            TSPlayer player = args.Player;
            offline.Add(player.Name);
            offlineindex[player.Index] = true;

            player.mute = true; //Just for saftey ;)

            //Team Update
            int team = player.TPlayer.team;
            player.TPlayer.team = 0;
            NetMessage.SendData(45, -1, -1, "", player.Index, 0f, 0f, 0f, 0);
            player.TPlayer.team = team;

            //Player Update
            player.TPlayer.position.X = 0f;
            player.TPlayer.position.Y = 0f;
            NetMessage.SendData(13, -1, -1, "", player.Index, 0f, 0f, 0f, 0);

            TSPlayer.All.SendMessage(string.Format("{0} left", player.Name), Color.Yellow);
        }

        protected void flogon(CommandArgs args)
        {
            TSPlayer player = args.Player;
            player.mute = false;
            offline.Remove(player.Name);
            offlineindex[player.Index] = false;
            TSPlayer.All.SendMessage(string.Format("{0} has joined", player.Name), Color.Yellow);
        }
    }
}
