using System;
using Dalamud.Plugin.Ipc;
using FeteTimer.Game;

namespace FeteTimer.IPC
{
    public class TeleporterTeleportBackend : ITeleportBackend
    {
        private bool available;
        private long timeSinceLastCheck;

        private ICallGateSubscriber<bool>? isInitialized;
        private ICallGateSubscriber<uint, byte, bool>? teleport;

        public TeleporterTeleportBackend()
        {
            try
            {
                isInitialized = Plugin.PluginInterface.GetIpcSubscriber<bool>("Teleport.ChatMessage");
                teleport = Plugin.PluginInterface.GetIpcSubscriber<uint, byte, bool>("Teleport");
            }
            catch (Exception ex)
            {
                Plugin.Logger.Debug($"FeteTimer: failed to subscribe to Teleporter IPC: {ex}");
            }
        }

        public string Name => "Teleporter";

        public bool Installed
        {
            get
            {
                if (timeSinceLastCheck + 5000 > Environment.TickCount64)
                    return available;

                timeSinceLastCheck = Environment.TickCount64;

                if (isInitialized == null)
                {
                    // Subscriber never constructed (ctor failed) — Teleporter is not usable.
                    available = false;
                    return available;
                }

                try
                {
                    isInitialized.InvokeFunc();
                    available = true;
                }
                catch
                {
                    available = false;
                }

                return available;
            }
        }

        public bool TryTeleportToFirmament()
        {
            try
            {
                return teleport?.InvokeFunc(FirmamentZone.FoundationAetheryteId, 0) ?? false;
            }
            catch (Exception ex)
            {
                Plugin.Logger.Warning(ex, "FeteTimer: Teleporter IPC teleport failed");
                return false;
            }
        }
    }
}
