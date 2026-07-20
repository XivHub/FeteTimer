using System;
using FeteTimer.Loc;

namespace FeteTimer.Game
{
    public enum SchedulePhase
    {
        /// <summary>No calibration data exists for the current World.</summary>
        Uncalibrated,
        /// <summary>A next-fête time is known and has not yet expired (including the grace window).</summary>
        CountingDown,
        /// <summary>The stored next-fête time is in the past beyond the grace window; re-calibration needed.</summary>
        Expired,
    }

    /// <summary>
    /// Pure timing logic — no Dalamud, no game, no unsafe. All inputs are BCL values.
    /// </summary>
    public static class SchedulePredictor
    {
        /// <summary>
        /// How long past <see cref="WorldCalibration.NextFeteUtc"/> we still treat the phase as
        /// CountingDown rather than Expired, to absorb server-side scheduling jitter.
        /// </summary>
        public static readonly TimeSpan ExpiredGrace = TimeSpan.FromSeconds(90);

        /// <summary>
        /// Determines the current schedule phase for a World.
        /// </summary>
        /// <param name="cal">Calibration data for the World, or null if uncalibrated.</param>
        /// <param name="nowUtc">Current UTC time.</param>
        /// <param name="nextFeteUtc">
        /// The stored next-fête time when phase is CountingDown or Expired; null when Uncalibrated.
        /// </param>
        public static SchedulePhase EvaluatePhase(
            WorldCalibration? cal,
            DateTime nowUtc,
            out DateTime? nextFeteUtc)
        {
            if (cal == null)
            {
                nextFeteUtc = null;
                return SchedulePhase.Uncalibrated;
            }

            if (cal.Value.NextFeteUtc + ExpiredGrace < nowUtc)
            {
                nextFeteUtc = cal.Value.NextFeteUtc;
                return SchedulePhase.Expired;
            }

            nextFeteUtc = cal.Value.NextFeteUtc;
            return SchedulePhase.CountingDown;
        }

        /// <summary>
        /// Constructs a fresh calibration record from the most recent in-zone observation.
        /// Always overwrites any prior calibration — no index inference, no carry-forward logic.
        /// </summary>
        public static WorldCalibration BuildCalibration(
            uint worldId,
            string worldName,
            DateTime nextFeteUtc,
            CalibrationConfidence conf,
            DateTime nowUtc)
        {
            return new WorldCalibration
            {
                WorldId = worldId,
                WorldName = worldName,
                NextFeteUtc = nextFeteUtc,
                LastCalibratedUtc = nowUtc,
                Confidence = conf,
            };
        }

        /// <summary>
        /// Formats a countdown duration into a human-readable string.
        /// Negative or sub-second values are clamped to "now".
        /// Examples: "1d 12h 43m", "1h 58m", "4m 12s", "now".
        /// </summary>
        public static string FormatCountdown(TimeSpan remaining)
        {
            if (remaining.TotalSeconds < 1)
                return Strings.Now;

            if (remaining.TotalDays >= 1)
            {
                int d = (int)remaining.TotalDays;
                return $"{d}d {remaining.Hours}h {remaining.Minutes}m";
            }

            if (remaining.TotalHours >= 1)
            {
                int h = (int)remaining.TotalHours;
                int m = remaining.Minutes;
                return $"{h}h {m}m";
            }

            if (remaining.TotalMinutes >= 1)
            {
                int m = (int)remaining.TotalMinutes;
                int s = remaining.Seconds;
                return $"{m}m {s}s";
            }

            // Less than 1 minute but at least 1 second.
            return $"{remaining.Seconds}s";
        }

    }
}
