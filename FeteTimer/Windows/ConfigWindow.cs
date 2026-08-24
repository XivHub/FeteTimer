using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FeteTimer.Alerts;
using FeteTimer.Loc;
using XivHubPluginKit.UI;

namespace FeteTimer.Windows
{
    public class ConfigWindow : Window, IDisposable
    {
        public ConfigWindow(Configuration configuration) : base($"{Strings.SettingsTitle}###FeteTimerConfig")
        {
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(340, 300),
                MaximumSize = new Vector2(700, 900),
            };
        }

        public void Dispose() { }

        public override void Draw()
        {
            DrawChannels();
            ImGui.Separator();
            DrawLeadTimes();
            ImGui.Separator();
            DrawSound();
            ImGui.Separator();
            DrawAppearance();
            ImGui.Separator();
            DrawDeveloperSection();
            ImGui.Spacing();
            DrawDisclaimer();
        }

        private void DrawChannels()
        {
            ImGui.TextDisabled(Strings.HdrAlertChannels);

            var chatEnabled = Plugin.C.ChatEnabled;
            if (ImGui.Checkbox(Strings.ChkChat, ref chatEnabled))
            {
                Plugin.C.ChatEnabled = chatEnabled;
                Plugin.C.Save();
            }

            var toastEnabled = Plugin.C.ToastEnabled;
            if (ImGui.Checkbox(Strings.ChkToast, ref toastEnabled))
            {
                Plugin.C.ToastEnabled = toastEnabled;
                Plugin.C.Save();
            }

            var soundEnabled = Plugin.C.SoundEnabled;
            if (ImGui.Checkbox(Strings.ChkSound, ref soundEnabled))
            {
                Plugin.C.SoundEnabled = soundEnabled;
                Plugin.C.Save();
            }

            var dtrEnabled = Plugin.C.DtrEnabled;
            if (ImGui.Checkbox(Strings.ChkDtr, ref dtrEnabled))
            {
                Plugin.C.DtrEnabled = dtrEnabled;
                Plugin.C.Save();
            }
        }

        private void DrawLeadTimes()
        {
            ImGui.TextDisabled(Strings.HdrLeadTimes);
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(Strings.LeadTimesTooltip);

            var toRemove = -1;
            for (var i = 0; i < Plugin.C.LeadTimesSeconds.Count; i++)
            {
                var v = Plugin.C.LeadTimesSeconds[i];
                ImGui.SetNextItemWidth(120);
                if (ImGui.InputInt($"##lead{i}", ref v))
                {
                    Plugin.C.LeadTimesSeconds[i] = v;
                    Plugin.C.Save();
                }
                ImGui.SameLine();
                if (ImGui.Button($"-##removelead{i}"))
                    toRemove = i;
            }

            if (toRemove >= 0)
            {
                Plugin.C.LeadTimesSeconds.RemoveAt(toRemove);
                Plugin.C.Save();
            }

            if (ImGui.Button(Strings.BtnAddLead))
            {
                Plugin.C.LeadTimesSeconds.Add(0);
                Plugin.C.Save();
            }
        }

        private void DrawSound()
        {
            ImGui.TextDisabled(Strings.HdrSound);

            var soundId = (int)Plugin.C.SoundEffectId;
            ImGui.SetNextItemWidth(120);
            if (ImGui.InputInt(Strings.LblSoundId, ref soundId))
            {
                Plugin.C.SoundEffectId = (uint)Math.Max(0, soundId);
                Plugin.C.Save();
            }

            ImGui.SameLine();
            if (ImGui.Button(Strings.BtnTestSound))
                AlertChannels.FireSound();

            if (ImGui.Button(Strings.BtnTestAlert))
                AlertChannels.FireAllEnabled(Strings.AlertTitle, Strings.TestAlertMessage);
        }

        /// <summary>
        /// The theme editor is generated from the kit's option table, so this stays
        /// one call however many themed values the kit grows.
        /// </summary>
        private static void DrawAppearance()
        {
            ImGui.TextDisabled(Strings.HdrAppearance);
            ImGui.TextColored(HubStyle.Faint, Strings.AppearanceDesc);
            ImGui.Spacing();
            HubThemeEditor.Draw(Plugin.ThemeConfig);
        }

        private void DrawDeveloperSection()
        {
            if (ImGui.CollapsingHeader(Strings.HdrDeveloper))
            {
                ImGui.TextDisabled(Strings.DevDesc);

                var devLog = Plugin.C.DevLog;
                if (ImGui.Checkbox(Strings.ChkDevLog, ref devLog))
                {
                    Plugin.C.DevLog = devLog;
                    Plugin.C.Save();
                }

                var url = Plugin.C.DevLogUrl;
                if (ImGui.InputText(Strings.LblDevUrl, ref url, 256))
                {
                    Plugin.C.DevLogUrl = url;
                    Plugin.C.Save();
                }

                ImGui.TextDisabled(Strings.DevUrlExample);
            }
        }

        private void DrawDisclaimer()
        {
            using (ImRaii.PushColor(ImGuiCol.Text, HubStyle.Faint))
            {
                ImGui.TextWrapped(Strings.Disclaimer);
            }
        }
    }
}
