using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPTCPGecko;

namespace SmashDiscordRichPresence
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get user ip address
            string ip;
            if (File.Exists("ip.txt"))
            {
                ip = File.ReadAllText("ip.txt").Trim();
            }
            else if (args.Length > 0)
            {
                ip = args[0];
            }
            else
            {
                Console.Write("Ip: ");
                ip = Console.ReadLine();
            }

            // Setup discord
            DiscordController discord = new DiscordController();
            discord.Initialize();
            DiscordRpc.UpdatePresence(discord.presence);

            // Setup tcpgecko using ip
            TCPGecko gecko = new TCPGecko(ip, 7331);
            if (!gecko.Connect())
            {
                Console.WriteLine("Failed to connect.");
                return;
            }
            
            int[] players = new int[8];
            int stage = 0, mode = 0;

            while (true)
            {
                // Grab some values from memory
                try
                {
                    players[0] = (int)gecko.peek(0x1098EDEB) & 0xFF;
                    players[1] = (int)gecko.peek(0x1098EE6B) & 0xFF;
                    players[2] = (int)gecko.peek(0x1098EEEB) & 0xFF;
                    players[3] = (int)gecko.peek(0x1098EF6B) & 0xFF;
                    players[4] = (int)gecko.peek(0x1098EFEB) & 0xFF;
                    players[5] = (int)gecko.peek(0x1098F06B) & 0xFF;
                    players[6] = (int)gecko.peek(0x1098F0EB) & 0xFF;
                    players[7] = (int)gecko.peek(0x1098F16B) & 0xFF;
                    stage = (int)gecko.peek(0x1097577F) & 0xFF;
                    mode = (int)gecko.peek(0x1098B2AB) & 0xFF;
                }
                catch { } // 3DS mode crashes for some reason idk
                
                List<int> activePlayers = new List<int>();
                for (int i = 0; i < 8; i++)
                    if (players[i] != 0xFF)
                        activePlayers.Add(players[i]);

                if (Info.IN_GAME_MODES.Contains(mode) && Info.STAGE_IMAGE_KEYS.ContainsKey(stage))
                {
                    discord.presence = new DiscordRpc.RichPresence()
                    {
                        smallImageKey = "",
                        smallImageText = "",
                        largeImageKey = Info.STAGE_IMAGE_KEYS[stage],
                        largeImageText = Info.STAGE_NAMES[stage]
                    };
                }
                else if (mode == 0x2E)
                {
                    discord.presence = new DiscordRpc.RichPresence()
                    {
                        smallImageKey = "",
                        smallImageText = "",
                        largeImageKey = "stage_maker",
                        largeImageText = ""
                    };
                }
                else
                {
                    discord.presence = new DiscordRpc.RichPresence()
                    {
                        smallImageKey = "",
                        smallImageText = "",
                        largeImageKey = "smash",
                        largeImageText = ""
                    };
                }

                if (Info.MODE_DESCRIPTIONS.ContainsKey(mode))
                    discord.presence.state = Info.MODE_DESCRIPTIONS[mode];
                else
                    discord.presence.state = "In game";
                if (Info.SINGLEPLAYER_MODES.Contains(mode))
                {
                    if (activePlayers.Count >= 1 && Info.CHARACTER_NAMES.ContainsKey(activePlayers[0]))
                        discord.presence.details = $"Playing as {Info.CHARACTER_NAMES[activePlayers[0]]}";
                }
                else if (Info.IN_GAME_MODES.Contains(mode))
                {
                    if (activePlayers.Count == 1 && Info.CHARACTER_NAMES.ContainsKey(activePlayers[0]))
                        discord.presence.details = $"Playing as {Info.CHARACTER_NAMES[activePlayers[0]]}";
                    else if (activePlayers.Count == 2 && Info.CHARACTER_NAMES.ContainsKey(activePlayers[0]) && Info.CHARACTER_NAMES.ContainsKey(activePlayers[1]))
                        discord.presence.details = $"{Info.CHARACTER_NAMES[activePlayers[0]]} vs {Info.CHARACTER_NAMES[activePlayers[1]]}";
                }

                DiscordRpc.UpdatePresence(discord.presence);

                // Sleep 5 seconds between updating stuff
                System.Threading.Thread.Sleep(5000);
            }
            gecko.Disconnect();
            Console.ReadLine();
        }
    }
}
