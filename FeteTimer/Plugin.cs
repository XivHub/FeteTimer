using System;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.EzDTR;
using FeteTimer.Game;
using FeteTimer.IPC;
using FeteTimer.Scheduler;
using FeteTimer.Windows;
using XivHubPluginKit;

namespace FeteTimer
{
    public sealed class Plugin : IDalamudPlugin
    {
        private const string CommandName = "/fetetimer";

        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IDataManager DataManager { get; private set; } = null!;
        [PluginService] public static IPluginLog Logger { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static IFramework Framework { get; private set; } = null!;
        [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
        [PluginService] public static IGameGui GameGui { get; private set; } = null!;
        [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
        [PluginService] public static INotificationManager NotificationManager { get; private set; } = null!;

        public static Configuration C { get; private set; } = null!;
        public static DevTelemetry? Telemetry { get; private set; }
        public static ITeleportBackend Teleport { get; private set; } = null!;

        public WindowSystem WindowSystem = new("FeteTimer");
        private readonly MainWindow mainWindow;
        private readonly ConfigWindow configWindow;
        private EzDtr? dtr;

        public Plugin()
        {
            ECommonsMain.Init(PluginInterface, this, Module.DalamudReflector);

            var lifestream = new LifestreamTeleportBackend();
            var teleporter = new TeleporterTeleportBackend();
            Teleport = new CompositeTeleportBackend(lifestream, teleporter);

            var config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            config.Initialize(PluginInterface);
            C = config;

            Telemetry = new DevTelemetry("FeteTimer", () => C.DevLog, () => C.DevLogUrl,
                err => Logger.Debug($"DevLog post failed: {err}"));

            FeteScheduler.Init();

            mainWindow = new MainWindow(C);
            configWindow = new ConfigWindow(C);
            WindowSystem.AddWindow(mainWindow);
            WindowSystem.AddWindow(configWindow);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open the FeteTimer window."
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
            Framework.Update += OnFrameworkUpdate;
            ClientState.Logout += OnLogout;

            // Server Info (DTR) bar entry: shows the next-fête countdown / "in progress"; click opens
            // the window. Shown only when enabled and there's something to show.
            dtr = new EzDtr(
                BuildDtrText,
                ToggleMainUi,
                "FeteTimer",
                () => C.DtrEnabled && (FeteScheduler.InProgress || FeteScheduler.CurrentPhase == SchedulePhase.CountingDown));
        }

        private static SeString BuildDtrText()
        {
            if (FeteScheduler.InProgress)
                return new SeString(new TextPayload(Loc.Strings.DtrNow));

            var next = FeteScheduler.NextFeteUtc;
            if (FeteScheduler.CurrentPhase == SchedulePhase.CountingDown && next.HasValue)
                return new SeString(new TextPayload(Loc.Strings.DtrCountdown(SchedulePredictor.FormatCountdown(next.Value - DateTime.UtcNow))));

            return new SeString(new TextPayload(Loc.Strings.DtrIdle));
        }

        private void OnFrameworkUpdate(IFramework framework) => FeteScheduler.Tick();

        private void OnLogout(int type, int code)
        {
            FeteScheduler.Reset();
        }

        public void Dispose()
        {
            Framework.Update -= OnFrameworkUpdate;
            ClientState.Logout -= OnLogout;
            FeteScheduler.Reset();
            dtr?.Dispose();
            dtr = null;
            Telemetry?.Dispose();
            Telemetry = null;
            Teleport = null!;

            PluginInterface.UiBuilder.Draw -= DrawUI;
            PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
            PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;

            WindowSystem.RemoveAllWindows();
            mainWindow.Dispose();
            configWindow.Dispose();

            CommandManager.RemoveHandler(CommandName);

            ECommonsMain.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            var a = args.Trim();
            if (a.StartsWith("dump", StringComparison.OrdinalIgnoreCase))
            {
                var addon = a.Length > 4 ? a.Substring(4).Trim() : string.Empty;
                if (string.IsNullOrEmpty(addon))
                    addon = "_ToDoList";
                Game.NodeDumper.Dump(addon);
                return;
            }

            mainWindow.IsOpen = true;
        }

        private void ToggleMainUi() => mainWindow.Toggle();

        private void ToggleConfigUi() => configWindow.Toggle();

        private void DrawUI() => WindowSystem.Draw();
    }
}
