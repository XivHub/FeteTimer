using System;
using ECommons.EzIpcManager;
using ECommons.Reflection;

namespace FeteTimer.IPC
{
    public class LifestreamTeleportBackend : ITeleportBackend
    {
        public LifestreamTeleportBackend() => EzIPC.Init(this, "Lifestream", SafeWrapper.AnyException);

        public string Name => "Lifestream";

        public bool Installed => DalamudReflector.TryGetDalamudPlugin("Lifestream", out _, false, true);

        /// <summary>Lifestream.ExecuteCommand(args) — runs `/li &lt;args&gt;`.</summary>
        [EzIPC] public readonly Action<string> ExecuteCommand = null!;

        /// <summary>Lifestream.IsBusy() — true while a travel sequence is already running.</summary>
        [EzIPC] public readonly Func<bool> IsBusy = null!;

        public bool TryTeleportToFirmament()
        {
            try
            {
                // Don't stack a request on top of an in-progress travel sequence.
                if (IsBusy())
                    return false;

                // `/li Firmament` runs Lifestream's built-in Firmament alias: teleport to the Foundation
                // aetheryte, THEN take the aethernet into The Firmament — works from anywhere. (The raw
                // AethernetTeleportToFirmament IPC only succeeds when already in Foundation aetheryte range,
                // and returns true on enqueue even when the queued aethernet step later fails.)
                ExecuteCommand("Firmament");
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Logger.Warning(ex, "FeteTimer: Lifestream teleport failed");
                return false;
            }
        }
    }
}
