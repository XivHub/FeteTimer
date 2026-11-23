using System;
using System.Text.RegularExpressions;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FeteTimer.Game
{
    /// <summary>
    /// PRIMARY calibration source: reads the next-fête countdown text from the in-game objective/duty
    /// addon while the player is in the Firmament. Observed format (rest period): the duty list shows
    /// e.g. "Fêtes commencing in 1d 12h 43m"; active-period text is the same shape with smaller units.
    ///
    /// The countdown text is found by auto-discovery — every text node of the addon is scanned and the
    /// first whose text parses as a duration (d/h/m/s tokens, prefix-tolerant) is used. A specific
    /// <see cref="CountdownNodeId"/> can be set to skip the scan once the exact node is known.
    ///
    /// Remaining unknown (V2): that the addon is named "_ToDoList". All game-memory access is unsafe
    /// and the whole body is wrapped in try/catch: any null pointer, missing node, parse miss, or
    /// exception returns false and the FateManager fallback carries.
    /// </summary>
    public sealed class ToDoListCalibrationSource : ICalibrationSource
    {
        /// <summary>
        /// Addon expected to hold the Firmament next-fête countdown (the duty/objective list).
        /// V2: not yet hard-confirmed; if wrong, the scan finds nothing and the fallback carries.
        /// </summary>
        private const string AddonName = "_ToDoList";

        /// <summary>
        /// Optional override: the text node id carrying the countdown. 0 = auto-discover by scanning
        /// all text nodes (the default). Set to a real id to skip the scan once confirmed via
        /// `/fetetimer dump`.
        /// </summary>
        private const uint CountdownNodeId = 0;

        /// <summary>
        /// Matches a "&lt;number&gt;&lt;unit&gt;" token. The English client writes d/h/m/s
        /// ("Fêtes commencing in 1d 12h 43m"); the Japanese client writes 日/時間/分/秒
        /// ("次回のFête開催まで15時間45分"). Scanning for tokens anywhere in the string tolerates the
        /// label in front of the countdown in either language.
        /// </summary>
        private static readonly Regex TokenRegex = new(@"(\d+)\s*(日|時間|分|秒|[dhms])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>Substring (case-insensitive) that marks an active fête, e.g. "Fêtes are in progress!".</summary>
        private const string InProgressMarker = "in progress";

        public string SourceName => "todolist";

        public bool IsFeteInProgress()
        {
            try
            {
                return ScanForInProgress();
            }
            catch (Exception ex)
            {
                Plugin.Logger.Debug($"[FeteTimer] ToDoList in-progress read failed: {ex.Message}");
                return false;
            }
        }

        private static unsafe bool ScanForInProgress()
        {
            var addon = Plugin.GameGui.GetAddonByName<AtkUnitBase>(AddonName);
            if (addon == null || !addon->IsVisible)
                return false;

            var found = false;
            NodeWalker.WalkTextNodes(addon, (id, text) =>
            {
                if (!found && text.IndexOf(InProgressMarker, StringComparison.OrdinalIgnoreCase) >= 0)
                    found = true;
            });
            return found;
        }

        public bool TryGetNextFeteUtc(DateTime nowUtc, out DateTime nextFeteUtc, out CalibrationConfidence confidence)
        {
            nextFeteUtc = default;
            confidence = CalibrationConfidence.None;

            try
            {
                var text = ReadCountdownText(CountdownNodeId);
                if (!TryParseCountdown(text, out var parsed))
                    return false;

                nextFeteUtc = nowUtc + parsed;
                confidence = CalibrationConfidence.AddonText;
                return true;
            }
            catch (Exception ex)
            {
                // Debug, not Error: a transient bad read must not spam the log every tick.
                Plugin.Logger.Debug($"[FeteTimer] ToDoList calibration read failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Parses a countdown like "1d 12h 43m", "58m", "2h 5m 30s" or "15時間45分" by summing the unit tokens found
        /// anywhere in the text (any surrounding label is ignored). Returns false if no positive
        /// duration is present.
        /// </summary>
        internal static bool TryParseCountdown(string? text, out TimeSpan span)
        {
            span = default;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var matches = TokenRegex.Matches(text);
            if (matches.Count == 0)
                return false;

            int d = 0, h = 0, m = 0, s = 0;
            foreach (Match mt in matches)
            {
                if (!int.TryParse(mt.Groups[1].Value, out var val))
                    continue;

                switch (mt.Groups[2].Value.ToLowerInvariant())
                {
                    case "d" or "日": d = val; break;
                    case "h" or "時間": h = val; break;
                    case "m" or "分": m = val; break;
                    case "s" or "秒": s = val; break;
                }
            }

            span = new TimeSpan(d, h, m, s);
            return span > TimeSpan.Zero;
        }

        /// <summary>
        /// Reads the countdown text: a specific node when <paramref name="nodeId"/> is set, otherwise
        /// the first text node whose text parses as a duration. Null when unavailable. Caller wraps
        /// this in try/catch.
        /// </summary>
        private static unsafe string? ReadCountdownText(uint nodeId)
        {
            var addon = Plugin.GameGui.GetAddonByName<AtkUnitBase>(AddonName);
            if (addon == null || !addon->IsVisible)
                return null;

            if (nodeId != 0)
            {
                var node = addon->GetTextNodeById(nodeId);
                if (node == null || !node->NodeText.StringPtr.HasValue)
                    return null;

                return MemoryHelper.ReadSeStringNullTerminated((nint)node->NodeText.StringPtr.Value).TextValue;
            }

            // Auto-discover: first text node whose text parses as a countdown duration.
            string? found = null;
            NodeWalker.WalkTextNodes(addon, (id, text) =>
            {
                if (found == null && TryParseCountdown(text, out _))
                    found = text;
            });
            return found;
        }
    }
}
