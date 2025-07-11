using Discord.WebSocket;
using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SL_Bot
{
    public class Program
    {
        public static Program Inst { get; set; } = null!;

        public DiscordSocketClient _client;

        public int oldcount = 0;

        public static void Log(string v) => SL_Bot.Log(v);

        public int newcount => SL_Bot.LocalMaxPlayers();

        public int MaxPlayers => SL_Bot.LocalMaxPlayers();

        public HashSet<KeyValuePair<ulong, int>> Numbers => SL_Bot.Singleton.Config.Numbers;

        public string Command => ".slserver" + SL_Bot.Singleton.Config.ServerNumber;

        public static async Task Main(string[] args)
        {
            Log("Main Starts");
            var program = new Program();
            await program.RunBotAsync();
        }

        public async Task RunBotAsync()
        {
            Log("Async Starts");
            Inst = this;
            _client = new DiscordSocketClient();
            _client.Log += LogAsync;
            _client.MessageReceived += MessageReceivedAsync;

            await _client.LoginAsync(TokenType.Bot, SL_Bot.Singleton.Config.Token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        public async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot)
                return;

            if (message.Content.ToLower().Length < Command.Length + 1)
                return;

            if (!(message.Content.ToLower().Remove(Command.Length + 1) == Command + " "))
                return;

            if (!int.TryParse(message.Content.Replace(Command + " ", ""), out int result))
                return;

            if ((result <= 0) || (result >= (MaxPlayers + 1)))
                return;

            bool remove = false;
            bool addnew = false;

            KeyValuePair<ulong, int> remov = new();
            KeyValuePair<ulong, int> addne = new();

            foreach (var item in Numbers)
            {
                if (item.Key == message.Author.Id)
                {
                    if (item.Value == result)
                    {
                        remove = true;
                        remov = item;
                        await message.Channel.SendMessageAsync($"Alright {GetDiscordPing(message.Author)} , I will no longer notify you when the server reaches {item.Value} players.");
                        break;
                    }
                    remove = true;
                    addnew = true;
                    remov = item;
                    addne = new KeyValuePair<ulong, int>(message.Author.Id, result);
                    await message.Channel.SendMessageAsync($"Alright {GetDiscordPing(message.Author)} , I will notify you when the sever reaches {result} players instead of {item.Value} players.");
                    break;
                }
            }

            if (remove)
                Numbers.Remove(remov);

            if (addnew)
                Numbers.Add(addne);

            if (remove)
                return;

            Numbers.Add(new KeyValuePair<ulong, int>(message.Author.Id, result));
            await message.Channel.SendMessageAsync($"Alright {GetDiscordPing(message.Author)} , I will notify you when the sever reaches {result} players.");
        }

        public string GetDiscordPing(SocketUser ev)
        {
            return "<@" + ev.Id + ">";
        }

        public async Task Update()
        {
            Log("Update Starts");

            Status();

            HashSet<ulong> ids = new();

            foreach (var item in Numbers)
            {
                if (item.Value <= oldcount)
                    continue;

                if (item.Value > newcount)
                    continue;

                ids.Add(item.Key);
            }

            foreach (var item in ids)
            {
                var user = _client.GetUserAsync(item).Result;

                await UserExtensions.SendMessageAsync(user, $"The server has reached a playercount of {newcount}.");
            }

            oldcount = newcount;
        }

        public void Status()
        {
            Log("Status Starts");

            if (newcount.Equals(0))
            {
                _client.SetStatusAsync(UserStatus.Idle);
                _client.SetCustomStatusAsync($"😊 | Server #{SL_Bot.Singleton.Config.ServerNumber} - empty");
            }

            _client.SetStatusAsync(UserStatus.Offline);
            _client.SetCustomStatusAsync($"🤩 | Server #{SL_Bot.Singleton.Config.ServerNumber} - {newcount}/{MaxPlayers} Players");
        }

        public Task LogAsync(LogMessage log)
        {
            return Task.CompletedTask;
        }
    }
}
