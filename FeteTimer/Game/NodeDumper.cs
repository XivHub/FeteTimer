using System;
using System.Text;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FeteTimer.Game
{
    /// <summary>
    /// Debug helper for the `/fetetimer dump [addon]` command: logs every non-empty text node of an
    /// addon (id + text), marking any whose text parses as a countdown. Used to confirm which addon /
    /// node carries the Firmament next-fête countdown (V2/V3).
    /// </summary>
    internal static unsafe class NodeDumper
    {
        internal static void Dump(string addonName)
        {
            try
            {
                var addon = Plugin.GameGui.GetAddonByName<AtkUnitBase>(addonName);
                if (addon == null)
                {
                    Plugin.Logger.Information($"[FeteTimer] dump: addon '{addonName}' not found / not loaded.");
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine($"[FeteTimer] text nodes of '{addonName}' (visible={addon->IsVisible}):");
                var count = 0;
                NodeWalker.WalkTextNodes(addon, (id, text) =>
                {
                    if (string.IsNullOrWhiteSpace(text))
                        return;

                    count++;
                    var mark = ToDoListCalibrationSource.TryParseCountdown(text, out var span)
                        ? $"   <== parses as countdown ({span})"
                        : string.Empty;
                    sb.AppendLine($"  node {id}: \"{text.Replace("\n", "\\n")}\"{mark}");
                });

                if (count == 0)
                    sb.AppendLine("  (no non-empty text nodes found)");

                Plugin.Logger.Information(sb.ToString());
            }
            catch (Exception ex)
            {
                Plugin.Logger.Warning(ex, "[FeteTimer] dump failed");
            }
        }
    }
}
