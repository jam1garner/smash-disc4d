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
        static int z(int x)
        {
            if (x != 0xFF)
                return 1;
            return 0;
        }

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
            
            int player1 = 0, player2 = 0, player3 = 0, player4 = 0, player5 = 0, player6 = 0, player7 = 0, player8 = 0;
            int stage = 0, mode = 0;

            while (true)
            {
                // Grab some values from memory
                try
                {
                    player1 = (int)gecko.peek(0x1098EDEB) & 0xFF;
                    player2 = (int)gecko.peek(0x1098EE6B) & 0xFF;
                    player3 = (int)gecko.peek(0x1098EEEB) & 0xFF;
                    player4 = (int)gecko.peek(0x1098EF6B) & 0xFF;
                    player5 = (int)gecko.peek(0x1098EFEB) & 0xFF;
                    player6 = (int)gecko.peek(0x1098F06B) & 0xFF;
                    player7 = (int)gecko.peek(0x1098F0EB) & 0xFF;
                    player8 = (int)gecko.peek(0x1098F16B) & 0xFF;
                    stage = (int)gecko.peek(0x1097577F) & 0xFF;
                    mode = (int)gecko.peek(0x1098B2AB) & 0xFF;
                }
                catch {} // 3DS mode crashes for some reason idk

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

                if (Info.SINGLEPLAYER_MODES.Contains(mode) && Info.CHARACTER_NAMES.ContainsKey(0xFF))
                    discord.presence.details = $"Playing as {Info.CHARACTER_NAMES[player1]}";
                else if (Info.ONE_V_ONE_MODES.Contains(mode) && Info.CHARACTER_NAMES.ContainsKey(player1) && Info.CHARACTER_NAMES.ContainsKey(player2))
                    discord.presence.details = $"{Info.CHARACTER_NAMES[player1]} vs {Info.CHARACTER_NAMES[player2]}";

                DiscordRpc.UpdatePresence(discord.presence);

                // Sleep 5 seconds between updating stuff
                System.Threading.Thread.Sleep(5000);
            }
            gecko.Disconnect();
            Console.ReadLine();
        }
    }
}
