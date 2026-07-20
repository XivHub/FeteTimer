using System;

namespace FeteTimer.Game
{
    /// <summary>
    /// Calibration source that tries the primary (<see cref="ToDoListCalibrationSource"/>) first and,
    /// ONLY if it returns false, falls back to <see cref="FateManagerCalibrationSource"/>.
    /// There is no cross-check between the two and no combined confidence — whichever source
    /// succeeds first wins outright with its own confidence value.
    /// </summary>
    public sealed class CalibrationSourceChain : ICalibrationSource
    {
        private readonly ToDoListCalibrationSource primary = new();
        private readonly FateManagerCalibrationSource fallback = new();

        public string SourceName => "chain(todolist→fatemanager)";

        public bool TryGetNextFeteUtc(DateTime nowUtc, out DateTime nextFeteUtc, out CalibrationConfidence confidence)
        {
            if (primary.TryGetNextFeteUtc(nowUtc, out nextFeteUtc, out confidence))
                return true;

            return fallback.TryGetNextFeteUtc(nowUtc, out nextFeteUtc, out confidence);
        }

        public bool IsFeteInProgress()
        {
            return primary.IsFeteInProgress() || fallback.IsFeteInProgress();
        }
    }
}
