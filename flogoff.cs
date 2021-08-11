using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TShockAPI;
using Microsoft.Xna.Framework;
using TerrariaApi.Server;

namespace Flogoff
{
    [ApiVersion(2, 1)]
    public class FLogoff : TerrariaPlugin
    {
        private readonly List<string> _offline = new List<string>();
        private static readonly bool[] Offlineindex = new bool[256];

        public override Version Version => new Version("1.0.9");

        public override string Name => "Flogoff";
        public override string Author => "DCCoder";

        public override string Description => "Performs a fake logoff";

        public FLogoff(Main game)
            : base(game)
        {
            Order = -1;
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(this,OnServerChat);
            ServerApi.Hooks.NetSendData.Register(this,OnSendData);
            Commands.ChatCommands.Add(new Command("flogoff", FlogOn, "flogon"));
            Commands.ChatCommands.Add(new Command("flogoff", FlogOff, "flogoff"));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerChat.Deregister(this,OnServerChat);
                ServerApi.Hooks.NetSendData.Deregister(this,OnSendData);
            }

            base.Dispose(disposing);
        }

        private static void OnSendData(SendDataEventArgs e)
        {
            var list = new List<int>();
            for (var i = 0; i < 256; i++)
                if (Offlineindex[i])
                    list.Add(i);
            var msgId = e.MsgId;
            if (msgId <= PacketTypes.DoorUse)
            {
                if (msgId != PacketTypes.PlayerSpawn && msgId != PacketTypes.DoorUse) goto IL_D2;
            }
            else
            {
                switch (msgId)
                {
                    case PacketTypes.ProjectileNew:
                    case PacketTypes.ProjectileDestroy:
                        if (list.Contains(e.ignoreClient) && Offlineindex[e.ignoreClient])
                        {
                            e.Handled = true;
                        }

                        goto IL_D2;

                    case PacketTypes.NpcStrike:
                        goto IL_D2;

                    default:
                        switch (msgId)
                        {
                            case PacketTypes.EffectHeal:
                            case PacketTypes.Zones:
                                break;

                            default:
                                switch (msgId)
                                {
                                    case PacketTypes.PlayerAnimation:
                                    case PacketTypes.EffectMana:
                                    case PacketTypes.PlayerTeam:
                                        break;

                                    case PacketTypes.PlayerMana:
                                    //case PacketTypes.PlayerKillMe:
                                        goto IL_D2;

                                    default:
                                        goto IL_D2;
                                }

                                break;
                        }

                        break;
                }
            }

            if (list.Contains(e.number) && Offlineindex[e.number]) e.Handled = true;
            IL_D2:
            if (e.number >= 0 && e.number <= 255 && Offlineindex[e.number] && e.MsgId == PacketTypes.PlayerUpdate)
                e.Handled = true;
        }
        
        private void OnServerChat(ServerChatEventArgs args)
        {
            if (args.Handled) return;

            var player = TShock.Players[args.Buffer.whoAmI];

            if (args.Text.StartsWith("/tp") || args.Text.StartsWith("/playing"))
            {
                if (args.Text.Substring(1, 3) == "tp")
                {
                    var words = args.Text.Split();
                    var tpto = words[1];

                    var result = _offline.Find(off => off == tpto);

                    if (!string.IsNullOrEmpty(result))
                    {
                        args.Handled = true;
                        player.SendMessage("Invalid player!", Color.Red);
                        return;
                    }
                }

                if (args.Text.Substring(1, 7) == "playing")
                {
                    args.Handled = true;
                    var players = TShock.Players.ToList();
                    foreach (var indPlayer in players)
                    {
                        var result = _offline.Find(off => string.Equals(off, indPlayer.Name, StringComparison.CurrentCultureIgnoreCase));
                        if (result != null)
                        {
                            players.Remove(indPlayer);
                        }
                    }

                    player.SendMessage($"Current players: {players.Select(x => x.Name)}.", 255, 240, 20);
                }
            }
        }

        private void FlogOff(CommandArgs args)
        {
            var player = args.Player;
            _offline.Add(player.Name);
            Offlineindex[player.Index] = true;

            player.mute = true; //Just for saftey ;)

            //Team Update
            var team = player.TPlayer.team;
            player.TPlayer.team = 0;
            NetMessage.SendData(45,number:player.Index);
            player.TPlayer.team = team;

            //Player Update
            player.TPlayer.position.X = 0f;
            player.TPlayer.position.Y = 0f;
            NetMessage.SendData(13,number:player.Index);

            TSPlayer.All.SendMessage($"{player.Name} left", Color.Yellow);
        }

        private void FlogOn(CommandArgs args)
        {
            var player = args.Player;
            player.mute = false;
            _offline.Remove(player.Name);
            Offlineindex[player.Index] = false;
            TSPlayer.All.SendMessage($"{player.Name} has joined", Color.Yellow);
        }
    }
}