using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FeteTimer.Game;
using FeteTimer.Loc;
using FeteTimer.Scheduler;
using XivHubPluginKit.UI;

namespace FeteTimer.Windows
{
    public class MainWindow : Window, IDisposable
    {
        public MainWindow(Configuration configuration) : base("FeteTimer###FeteTimerMain")
        {
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(360, 200),
                MaximumSize = new Vector2(900, 1200),
            };
        }

        public void Dispose() { }

        public override void Draw()
        {
            DrawHeader();
            ImGui.Separator();
            DrawRestGapLine();
            ImGui.Spacing();
            DrawCalibrationsTable();
            ImGui.Spacing();
            DrawTeleportButton();
        }

        private void DrawHeader()
        {
            var worldName = FeteScheduler.CurrentWorldName;
            var phase = FeteScheduler.CurrentPhase;
            var nextFete = FeteScheduler.NextFeteUtc;

            var worldLabel = string.IsNullOrEmpty(worldName) ? "—" : worldName;
            ImGui.TextUnformatted(Strings.WorldLine(worldLabel));
            ImGui.SameLine();

            string countdownText;
            if (FeteScheduler.InProgress)
            {
                countdownText = Strings.InProgress;
            }
            else if (phase == SchedulePhase.CountingDown && nextFete.HasValue)
            {
                var remaining = nextFete.Value - DateTime.UtcNow;
                countdownText = SchedulePredictor.FormatCountdown(remaining);
            }
            else if (phase == SchedulePhase.Expired)
            {
                countdownText = Strings.ExpiredHint;
            }
            else
            {
                countdownText = Strings.CalibrateHint;
            }

            ImGui.TextUnformatted($"| {Strings.NextFeteLabel}: {countdownText}");
        }

        private void DrawRestGapLine()
        {
            using (ImRaii.PushColor(ImGuiCol.Text, HubStyle.Faint))
            {
                ImGui.TextWrapped(Strings.RestGapInfo);
            }
        }

        private void DrawCalibrationsTable()
        {
            using var table = ImRaii.Table("##calibrations", 4,
                ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp);
            if (!table) return;

            ImGui.TableSetupColumn(Strings.ColWorld);
            ImGui.TableSetupColumn(Strings.ColNextLocal);
            ImGui.TableSetupColumn(Strings.ColConfidence);
            ImGui.TableSetupColumn(Strings.ColLastCalibrated);
            ImGui.TableHeadersRow();

            foreach (var cal in Plugin.C.Calibrations.Values)
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(cal.WorldName);

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(cal.NextFeteUtc.ToLocalTime().ToString("g"));

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(cal.Confidence.ToString());

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(cal.LastCalibratedUtc.ToLocalTime().ToString("g"));
            }
        }

        private void DrawTeleportButton()
        {
            using (ImRaii.Disabled(!Plugin.Teleport.Installed))
            {
                if (ImGui.Button(Strings.TeleportButton))
                    Plugin.Teleport.TryTeleportToFirmament();
            }
        }
    }
}
