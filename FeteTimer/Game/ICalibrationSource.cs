using System;

namespace FeteTimer.Game
{
    /// <summary>
    /// A source that can attempt to derive the next Firmament fête time from live game state.
    /// Implementations MUST fail safe: any null pointer, missing node, parse miss, or exception
    /// returns false rather than throwing into the framework tick.
    /// </summary>
    public interface ICalibrationSource
    {
        /// <summary>
        /// Attempts to read the next-fête UTC time from this source.
        /// </summary>
        /// <param name="nowUtc">Current UTC time, supplied by the caller for deterministic math.</param>
        /// <param name="nextFeteUtc">The derived next-fête time on success; otherwise unspecified.</param>
        /// <param name="confidence">The quality of the derived value on success; otherwise unspecified.</param>
        /// <returns>True if a value was produced; false on any failure (caller must ignore the outs).</returns>
        bool TryGetNextFeteUtc(DateTime nowUtc, out DateTime nextFeteUtc, out CalibrationConfidence confidence);

        /// <summary>
        /// True if this source can tell a fête is currently in progress (active now, no next-fête
        /// countdown shown). Fail-safe: returns false on any null/parse/exception.
        /// </summary>
        bool IsFeteInProgress();

        /// <summary>Short identifier for logging / diagnostics.</summary>
        string SourceName { get; }
    }
}
