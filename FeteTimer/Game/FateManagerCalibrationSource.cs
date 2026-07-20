using System;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;

namespace FeteTimer.Game
{
    /// <summary>
    /// FALLBACK in-progress detector (best-effort): inspects <see cref="FateManager"/> for an active,
    /// currently-running fête. It cannot derive the *next* fête's start, so <see cref="TryGetNextFeteUtc"/>
    /// always returns false; it only contributes <see cref="IsFeteInProgress"/> as a backstop to the
    /// primary text scan.
    ///
    /// V4 GENUINE UNKNOWN: there is no confirmed way to identify "a Firmament fête" purely from
    /// FateManager fields, so this only confirms that *a* fate is running (scoped by the caller only
    /// consulting it while the player is inside the Firmament).
    ///
    /// All game-memory access is in unsafe blocks and wrapped in try/catch: any null pointer or
    /// exception returns false.
    /// </summary>
    public sealed class FateManagerCalibrationSource : ICalibrationSource
    {
        public string SourceName => "fatemanager";

        // FateManager cannot derive a future next-fête time; the countdown comes from the text source.
        public bool TryGetNextFeteUtc(DateTime nowUtc, out DateTime nextFeteUtc, out CalibrationConfidence confidence)
        {
            nextFeteUtc = default;
            confidence = CalibrationConfidence.None;
            return false;
        }

        public bool IsFeteInProgress()
        {
            try
            {
                return TryReadActiveFete();
            }
            catch (Exception ex)
            {
                Plugin.Logger.Debug($"[FeteTimer] FateManager in-progress read failed: {ex.Message}");
                return false;
            }
        }

        private static unsafe bool TryReadActiveFete()
        {
            var fm = FateManager.Instance();
            if (fm == null)
                return false;

            var fate = fm->CurrentFate;
            if (fate == null)
                return false;

            // V4 best-effort: confirm the current fate is actually running, with valid timing fields.
            if (fate->State != FateState.Running)
                return false;

            if (fate->StartTimeEpoch <= 0 || fate->Duration <= 0)
                return false;

            return true;
        }
    }
}
