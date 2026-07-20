using System;
using Dalamud.Interface.ImGuiNotification;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace FeteTimer.Alerts
{
    /// <summary>
    /// Stateless alert output channels. Reads its dependencies from the public static
    /// <see cref="Plugin"/> services and <see cref="Configuration"/> directly — no constructor
    /// injection (SealHunter pattern). Each channel self-guards on its enabled flag and is
    /// individually wrapped in try/catch so a single failing channel never breaks the others
    /// or the framework tick.
    /// </summary>
    internal static class AlertChannels
    {
        /// <summary>Fires every enabled channel. Used by the alert engine and the "Test alert" button.</summary>
        internal static void FireAllEnabled(string title, string msg)
        {
            FireChat(msg);
            FireToast(title, msg);
            FireSound();
        }

        /// <summary>Prints a chat-log line. No-op when chat alerts are disabled.</summary>
        internal static void FireChat(string msg)
        {
            if (!Plugin.C.ChatEnabled)
                return;

            try
            {
                Plugin.ChatGui.Print($"[FeteTimer] {msg}");
            }
            catch (Exception ex)
            {
                Plugin.Logger.Warning(ex, "FeteTimer: chat alert channel failed");
            }
        }

        /// <summary>Raises a Dalamud notification. No-op when toast alerts are disabled.</summary>
        internal static void FireToast(string title, string msg)
        {
            if (!Plugin.C.ToastEnabled)
                return;

            try
            {
                Plugin.NotificationManager.AddNotification(new Notification
                {
                    Title = title,
                    Content = msg,
                    Type = NotificationType.Info,
                    InitialDuration = TimeSpan.FromSeconds(5),
                });
            }
            catch (Exception ex)
            {
                Plugin.Logger.Warning(ex, "FeteTimer: toast alert channel failed");
            }
        }

        /// <summary>Plays the configured game sound effect. No-op when sound alerts are disabled.</summary>
        internal static unsafe void FireSound()
        {
            if (!Plugin.C.SoundEnabled)
                return;

            try
            {
                // PlayChatSoundEffect plays the <se.1>..<se.16> chat alert tones — the user-facing,
                // configurable 1–16 sound space (SoundEffectId default 1 = the simple click). The raw
                // PlaySoundEffect indexes a different table where id 1 is not that tone.
                UIGlobals.PlayChatSoundEffect(Plugin.C.SoundEffectId);
            }
            catch (Exception ex)
            {
                Plugin.Logger.Warning(ex, "FeteTimer: sound alert channel failed");
            }
        }
    }
}
