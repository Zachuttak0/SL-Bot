using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using System;
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
            Program.Main(thing);
            PlayerEvents.Joined += Joined;
            PlayerEvents.Left += Left;
            ServerEvents.RoundStarted += RoundStarted;
            ServerEvents.WaitingForPlayers += WaitingForPlayers;
        }

        string[] thing = [];

        public static int LocalPlayerCount() { return Server.PlayerCount; }

        public static int LocalMaxPlayers() { return Server.MaxPlayers; }

        public void WaitingForPlayers() { Timing.CallDelayed(60, () => Delay()); }

        public void Delay() { if (Server.PlayerCount == 0) { UpdateCount(); } }

        public void RoundStarted() { UpdateCount(); }

        public void Joined(PlayerJoinedEventArgs ev) { if (Round.IsRoundInProgress) { UpdateCount(); } }

        public void Left(PlayerLeftEventArgs ev) { if (Round.IsRoundInProgress) {  UpdateCount(); } }

        public async void UpdateCount() { await Program.Inst.Update(); }

        public static void Log(string v)
        {
            Console.Write(v);
        }
    }
}
