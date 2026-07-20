using Dalamud.Configuration;
using Dalamud.Plugin;
using FeteTimer.Game;
using System;
using System.Collections.Generic;

namespace FeteTimer
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        // Alert channel toggles.
        public bool ChatEnabled { get; set; } = true;
        public bool ToastEnabled { get; set; } = true;
        public bool SoundEnabled { get; set; } = true;

        // Lead times in seconds before each fête at which alerts fire (default: 5 min, 1 min, at-start).
        public List<int> LeadTimesSeconds { get; set; } = new List<int> { 300, 60, 0 };

        // Game sound effect id to play for sound alerts.
        public uint SoundEffectId { get; set; } = 1;

        // Per-World calibration data, keyed by World RowId (the current/visited World).
        // Populated when the player is in the Firmament and the countdown is visible.
        public Dictionary<uint, WorldCalibration> Calibrations { get; set; } = new Dictionary<uint, WorldCalibration>();

        // Show the next-fête countdown in the Server Info (DTR) bar.
        public bool DtrEnabled { get; set; } = true;

        // Developer: live telemetry to a local log server (off by default).
        public bool DevLog { get; set; } = false;
        public string DevLogUrl { get; set; } = "";

        [NonSerialized]
        private IDalamudPluginInterface? pluginInterface;

        public void Initialize(IDalamudPluginInterface pi)
        {
            this.pluginInterface = pi;

            // Defensive load-path guard: Dalamud deserializes JSON into a fresh object, so properties
            // that are absent from saved JSON arrive as null even though they have non-null default
            // initializers in the class body.  Replace nulls with empty collections before the rest
            // of the plugin touches them.
            Calibrations ??= new Dictionary<uint, WorldCalibration>();
            LeadTimesSeconds ??= new List<int> { 300, 60, 0 };

            // Config migration: Version 0 (pre-1.0, no saved defaults) → seed the alert defaults.
            if (Version < 1)
            {
                ChatEnabled = true;
                ToastEnabled = true;
                SoundEnabled = true;
                SoundEffectId = 1;
                if (LeadTimesSeconds.Count == 0)
                    LeadTimesSeconds.AddRange(new[] { 300, 60, 0 });
                Version = 1;
            }
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
