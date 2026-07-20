using System;
using ECommons;
using ECommons.GameHelpers;
using FeteTimer.Alerts;
using FeteTimer.Game;
using Lumina.Excel.Sheets;

namespace FeteTimer.Scheduler
{
    /// <summary>
    /// Drives calibration, phase evaluation, and alert dispatch.
    /// Called on IFramework.Update from Plugin.OnFrameworkUpdate.
    /// </summary>
    internal static class FeteScheduler
    {
        private static CalibrationSourceChain? Source;
        private static AlertEngine? Alerts;

        /// <summary>Last <see cref="Environment.TickCount64"/> at which a calibration was attempted.</summary>
        private static long lastCalibrateTick;

        /// <summary>
        /// Tolerance (seconds) for treating a fresh reading as the same fête. The in-game countdown is
        /// minute-resolution, so now+countdown sawtooths within a ~60s band; only re-store/re-arm when
        /// the target moves beyond this.
        /// </summary>
        private const double CalibrationToleranceSeconds = 90;

        /// <summary>World id the cached <see cref="CurrentWorldName"/> was resolved for (avoids a per-frame excel lookup + string alloc).</summary>
        private static uint cachedWorldId = uint.MaxValue;

        // ---- UI read-state (written on the framework thread, read by the windows) ----

        /// <summary>Current schedule phase for the resolved World.</summary>
        internal static SchedulePhase CurrentPhase { get; private set; } = SchedulePhase.Uncalibrated;

        /// <summary>Next-fête time for the resolved World, or null when uncalibrated.</summary>
        internal static DateTime? NextFeteUtc { get; private set; }

        /// <summary>True while a fête is currently in progress (live, in-zone only). Overrides the phase display.</summary>
        internal static bool InProgress { get; private set; }

        /// <summary>Row id of the World the local player is currently on.</summary>
        internal static uint CurrentWorldId { get; private set; }

        /// <summary>Display name of the World the local player is currently on.</summary>
        internal static string CurrentWorldName { get; private set; } = string.Empty;

        internal static void Init()
        {
            Source = new CalibrationSourceChain();
            Alerts = new AlertEngine();
            lastCalibrateTick = 0;
        }

        internal static void Tick()
        {
            // In this Dalamud API the local player lives on IObjectTable; ECommons Player.Object
            // resolves it (Svc.Objects.LocalPlayer).
            var localPlayer = Player.Object;
            if (!GenericHelpers.IsScreenReady() || localPlayer is null)
                return;

            // API 15: LocalPlayer.CurrentWorld is a RowRef<World>; .RowId is the World id,
            // .Value.Name.ExtractText() is the display name.
            var world = localPlayer.CurrentWorld;
            CurrentWorldId = world.RowId;
            // The World almost never changes mid-session; only re-resolve the name on a change to
            // avoid an excel lookup + SeString allocation every frame.
            if (world.RowId != cachedWorldId)
            {
                cachedWorldId = world.RowId;
                CurrentWorldName = world.Value.Name.ExtractText();
            }

            // Calibration + in-progress detection only happen in the Firmament, throttled to ~3s.
            if (!FirmamentZone.IsInFirmament((ushort)Plugin.ClientState.TerritoryType))
            {
                InProgress = false;
            }
            else if (Environment.TickCount64 - lastCalibrateTick > 3000)
            {
                lastCalibrateTick = Environment.TickCount64;
                InProgress = Source?.IsFeteInProgress() ?? false;

                // While a fête is in progress the text shows no countdown; don't try to calibrate.
                if (!InProgress)
                    TryCalibrate(CurrentWorldId, CurrentWorldName);
            }

            RunAlerts(CurrentWorldId);
        }

        private static void TryCalibrate(uint worldId, string worldName)
        {
            if (Source != null && Source.TryGetNextFeteUtc(DateTime.UtcNow, out var next, out var conf))
            {
                // The in-game countdown is minute-resolution, so now+countdown sawtooths within a ~60s
                // band. Only re-store (and re-arm alerts + save) when the target moves beyond tolerance —
                // i.e. a genuinely different fête or a correction — not for the per-tick jitter.
                if (Plugin.C.Calibrations.TryGetValue(worldId, out var existing)
                    && Math.Abs((existing.NextFeteUtc - next).TotalSeconds) <= CalibrationToleranceSeconds)
                    return;

                var cal = SchedulePredictor.BuildCalibration(worldId, worldName, next, conf, DateTime.UtcNow);
                Plugin.C.Calibrations[worldId] = cal;
                Plugin.C.Save();
                Alerts?.ResetWorld(worldId);

                Plugin.Logger.Info($"FeteTimer: calibrated {worldName} ({worldId}): next fête {cal.NextFeteUtc:u} ({cal.Confidence})");
                Plugin.Telemetry?.Log($"calibrated {worldName}: {cal.NextFeteUtc:u} ({cal.Confidence})");
            }
            else
            {
                Plugin.Logger.Debug($"FeteTimer: calibration attempt for {worldName} ({worldId}) found no source");
                Plugin.Telemetry?.Log("calibration attempt: no source");
            }
        }

        private static void RunAlerts(uint worldId)
        {
            // A fête happening right now overrides the stored countdown: surface it and fire no
            // next-fête alerts (the "at start" alert already fired as the countdown hit zero).
            if (InProgress)
            {
                NextFeteUtc = null;
                Plugin.Telemetry?.Snapshot(() => $"phase=InProgress world={CurrentWorldName}");
                return;
            }

            WorldCalibration? cal = Plugin.C.Calibrations.TryGetValue(worldId, out var stored) ? stored : null;

            CurrentPhase = SchedulePredictor.EvaluatePhase(cal, DateTime.UtcNow, out var next);
            NextFeteUtc = next;

            Plugin.Telemetry?.Snapshot(() => $"phase={CurrentPhase} world={CurrentWorldName} next={NextFeteUtc:u}");

            if (CurrentPhase != SchedulePhase.CountingDown)
            {
                // Expired or Uncalibrated: never fire alerts.
                return;
            }

            Alerts?.Evaluate(worldId, cal!.Value.NextFeteUtc, DateTime.UtcNow);
        }

        internal static void Reset()
        {
            CurrentPhase = SchedulePhase.Uncalibrated;
            NextFeteUtc = null;
            InProgress = false;
            CurrentWorldId = 0;
            CurrentWorldName = string.Empty;
            cachedWorldId = uint.MaxValue;
            lastCalibrateTick = 0;

            // Null-guarded: Reset may run on dispose before/without a successful Init.
            Alerts?.ResetAll();
        }
    }
}
