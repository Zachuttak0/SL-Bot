using Discord.WebSocket;
using Discord;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MEC;

namespace SL_Bot
{
    public class SL_Bot : Plugin<Config>
    {
        public static Plugin<Config> Singleton { get; set; } = null!;

        public override string Author { get; } = "Zachuttak0";

        public override string Description { get; } = "A plugin for controlling a discord bot";

        public override string Name { get; } = "SL-Bot";

        public override Version Version { get; } = new(1, 0, 0);

        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

        public override void Disable()
        {
            Singleton = null;
            PlayerEvents.Joined -= Joined;
            PlayerEvents.Left -= Left;
            ServerEvents.RoundStarted -= RoundStarted;
            ServerEvents.WaitingForPlayers -= WaitingForPlayers;
        }

        public override void Enable()
        {
            Singleton = this;
            PlayerEvents.Joined += Joined;
            PlayerEvents.Left += Left;
            ServerEvents.RoundStarted += RoundStarted;
            ServerEvents.WaitingForPlayers += WaitingForPlayers;
        }

        public void WaitingForPlayers() { Timing.CallDelayed(60, () => Delay()); }

        public void Delay() { if (Server.PlayerCount == 0) { UpdateCount(); } }

        public void RoundStarted() { UpdateCount(); }

        public void Joined(PlayerJoinedEventArgs ev) { if (Round.IsRoundInProgress) { UpdateCount(); } }

        public void Left(PlayerLeftEventArgs ev) { if (Round.IsRoundInProgress) {  UpdateCount(); } }

        public async void UpdateCount() { await Program.Inst.Update(); }

        public class Program
        {
            public static Program Inst { get; set; } = null!;

            private DiscordSocketClient _client;

            private int oldcount = 0;

            private int newcount => Server.PlayerCount;

            private HashSet<KeyValuePair<ulong, int>> Numbers => Singleton.Config.Numbers;

            private string Command => ".slserver" + Singleton.Config.ServerNumber;

            static async Task Main(string[] args)
            {
                var program = new Program();
                await program.RunBotAsync();
            }

            public async Task RunBotAsync()
            {
                Inst = this;
                _client = new DiscordSocketClient();
                _client.Log += LogAsync;
                _client.MessageReceived += MessageReceivedAsync;

                await _client.LoginAsync(TokenType.Bot, Singleton.Config.Token);
                await _client.StartAsync();

                await Task.Delay(-1);
            }

            private async Task MessageReceivedAsync(SocketMessage message)
            {
                if (message.Author.IsBot)
                    return;

                if (message.Content.ToLower().Length < Command.Length + 1)
                    return;

                if (!(message.Content.ToLower().Remove(Command.Length + 1) == Command + " "))
                    return;

                if (!int.TryParse(message.Content.Replace(Command + " ", ""), out int result))
                    return;

                if ((result <= 0) || (result >= (Server.MaxPlayers + 1)))
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

            private string GetDiscordPing(SocketUser ev)
            {
                return "<@" + ev.Id + ">";
            }

            public async Task Update()
            {
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

            private void Status()
            {
                if (newcount.Equals(0))
                {
                    _client.SetStatusAsync(UserStatus.Idle);
                    _client.SetCustomStatusAsync($"😊 | Server #{Singleton.Config.ServerNumber} - empty");
                }

                _client.SetStatusAsync(UserStatus.Offline);
                _client.SetCustomStatusAsync($"🤩 | Server #{Singleton.Config.ServerNumber} - {newcount}/{Server.MaxPlayers} Players");
            }

            private Task LogAsync(LogMessage log)
            {
                return Task.CompletedTask;
            }
        }
    }
}
