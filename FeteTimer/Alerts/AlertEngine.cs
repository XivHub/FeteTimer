using System;
using System.Collections.Generic;
using FeteTimer.Game;
using FeteTimer.Loc;

namespace FeteTimer.Alerts
{
    /// <summary>
    /// De-duplicating alert dispatcher. For a given (World, fête, lead-time) it fires the enabled
    /// channels exactly once. The fête identity is the next-fête timestamp ticks, so a fresh
    /// calibration to a new fête naturally produces new keys and re-arms the lead times.
    /// </summary>
    internal sealed class AlertEngine
    {
        /// <summary>
        /// Keys of alerts already fired, as (worldId, nextFeteUtcTicks, leadTimeSeconds).
        /// Including the fête timestamp guarantees one fire per (fête, lead-time);
        /// <see cref="ResetWorld"/> additionally purges a World's stale keys so the set stays bounded.
        /// </summary>
        private readonly HashSet<(uint WorldId, long NextFeteUtcTicks, int LeadTime)> fired = new();

        /// <summary>
        /// Evaluates the lead times for the current fête and fires any that have come due and not
        /// yet fired for this exact fête.
        /// </summary>
        internal void Evaluate(uint worldId, DateTime nextFeteUtc, DateTime nowUtc)
        {
            var remaining = (nextFeteUtc - nowUtc).TotalSeconds;

            foreach (var leadTime in Plugin.C.LeadTimesSeconds)
            {
                if (remaining > leadTime)
                    continue;

                var key = (worldId, nextFeteUtc.Ticks, leadTime);
                if (fired.Contains(key))
                    continue;

                var startingNow = leadTime <= 0 || (nextFeteUtc - nowUtc).TotalSeconds < 1;
                var msg = startingNow
                    ? Strings.AlertStartingNow
                    : Strings.AlertIn(SchedulePredictor.FormatCountdown(nextFeteUtc - nowUtc));

                AlertChannels.FireAllEnabled(Strings.AlertTitle, msg);
                Plugin.Telemetry?.Log($"alert fired world={worldId} lead={leadTime}s fete={nextFeteUtc:u} msg=\"{msg}\"");

                fired.Add(key);
            }
        }

        /// <summary>
        /// Removes every fired-key for a World. Called on (re-)calibration: it re-arms the new fête's
        /// lead times AND bounds <see cref="fired"/> growth by discarding the prior fête's stale keys.
        /// </summary>
        internal void ResetWorld(uint worldId)
        {
            fired.RemoveWhere(k => k.WorldId == worldId);
        }

        /// <summary>Clears all fired state (called on logout and dispose).</summary>
        internal void ResetAll()
        {
            fired.Clear();
        }
    }
}
