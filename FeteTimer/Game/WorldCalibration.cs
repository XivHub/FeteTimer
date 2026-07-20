using System;

namespace FeteTimer.Game
{
    public enum CalibrationConfidence
    {
        /// <summary>No calibration has been performed for this World.</summary>
        None,
        /// <summary>Next-fête time was estimated from the FateManager active-fate fields (fallback, less precise).</summary>
        FateManagerOnly,
        /// <summary>Next-fête time was read from the _ToDoList addon countdown text (primary, most reliable).</summary>
        AddonText,
    }

    [Serializable]
    public struct WorldCalibration
    {
        /// <summary>Dalamud / Lumina World row id.</summary>
        public uint WorldId { get; set; }

        /// <summary>Human-readable world name (stored for display; sourced from LocalPlayer.CurrentWorld at calibration time).</summary>
        public string WorldName { get; set; }

        /// <summary>UTC timestamp of the next Firmament Fête, as calibrated from the in-game countdown.</summary>
        public DateTime NextFeteUtc { get; set; }

        /// <summary>UTC timestamp of when this calibration was last written.</summary>
        public DateTime LastCalibratedUtc { get; set; }

        /// <summary>Source quality of the calibration data.</summary>
        public CalibrationConfidence Confidence { get; set; }
    }
}
