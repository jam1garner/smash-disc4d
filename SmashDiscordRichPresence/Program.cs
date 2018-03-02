using System;
using System.Collections.Generic;
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
            Console.Write("Ip: ");
            string ip = Console.ReadLine();

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

            while (true)
            {
                // Grab some values from memory
                int player1 = (int)gecko.peek(0x1098EDEB) & 0xFF;
                int player2 = (int)gecko.peek(0x1098EE6B) & 0xFF;
                int stage = (int)gecko.peek(0x1097577F) & 0xFF;
                int mode = (int)gecko.peek(0x1098B2AB) & 0xFF;

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
