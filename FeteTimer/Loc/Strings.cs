using System;

namespace FeteTimer.Loc
{
    /// <summary>
    /// Lightweight, dependency-free localization table for all user-facing strings. The language is
    /// not a plugin setting — it follows the Dalamud UI language (Spanish when that is set to "es",
    /// English otherwise).
    ///
    /// In-game terms are intentionally kept as-is: "Firmament" and "fête" are the names the player
    /// sees in their (English) client, and the scraped countdown comes from the game, so they are not
    /// translated here. FFXIV has no Spanish client; this only localizes the plugin's own text.
    /// </summary>
    public static class Strings
    {
        /// <summary>True when the Dalamud UI language is Spanish.</summary>
        public static bool IsSpanish
        {
            get
            {
                try
                {
                    var ui = Plugin.PluginInterface.UiLanguage;
                    return !string.IsNullOrEmpty(ui) && ui.StartsWith("es", StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }

        private static string T(string en, string es) => IsSpanish ? es : en;

        // ---- Main window ----
        public static string WorldLine(string name) => T($"World: {name}", $"Mundo: {name}");
        public static string NextFeteLabel => T("Next fête", "Próxima fête");
        public static string InProgress => T("A fête is in progress now!", "¡Hay una fête en curso ahora mismo!");
        public static string ExpiredHint => T("Visit the Firmament to re-confirm the next fête.", "Vuelve al Firmament para reconfirmar la próxima fête.");
        public static string CalibrateHint => T("Visit the Firmament to calibrate.", "Pásate por el Firmament para calibrar.");
        public static string RestGapInfo => T(
            "Fêtes run every ~2h during an active period (12 per period); after that there's a ~1–2 day rest. Re-enter the Firmament to re-confirm the next fête.",
            "Las fêtes salen cada ~2 h durante un periodo activo (12 por periodo); luego hay un descanso de ~1–2 días. Vuelve a entrar al Firmament para reconfirmar la próxima fête.");
        public static string ColWorld => T("World", "Mundo");
        public static string ColNextLocal => T("Next fête (local)", "Próxima fête (local)");
        public static string ColConfidence => T("Confidence", "Fiabilidad");
        public static string ColLastCalibrated => T("Last calibrated", "Última calibración");
        public static string TeleportButton => T("Teleport to Firmament", "Ir al Firmament");

        // ---- Config window ----
        public static string SettingsTitle => T("FeteTimer Settings", "Ajustes de FeteTimer");
        public static string DevUrlExample => T("e.g. http://192.168.88.248:9999/log", "p. ej. http://192.168.88.248:9999/log");
        public static string HdrAlertChannels => T("Alert channels", "Canales de aviso");
        public static string ChkChat => T("Chat messages", "Mensajes de chat");
        public static string ChkToast => T("Dalamud notifications (toast)", "Notificaciones de Dalamud (toast)");
        public static string ChkSound => T("Sound effect", "Efecto de sonido");
        public static string ChkDtr => T("Show countdown in Server Info bar", "Mostrar la cuenta atrás en la barra de Server Info");
        public static string HdrLeadTimes => T("Lead times", "Antelación");
        public static string LeadTimesTooltip => T("Seconds before each fête to alert; 0 = at start.", "Segundos antes de cada fête para avisarte; 0 = justo al empezar.");
        public static string BtnAddLead => T("Add lead time", "Añadir antelación");
        public static string HdrSound => T("Sound", "Sonido");
        public static string LblSoundId => T("Sound effect ID", "ID del efecto de sonido");
        public static string BtnTestSound => T("Test sound", "Probar sonido");
        public static string BtnTestAlert => T("Test alert", "Probar aviso");
        public static string HdrDeveloper => T("Developer", "Desarrollo");
        public static string DevDesc => T("Streams live activity + state snapshots to a local log server.", "Envía la actividad y el estado en tiempo real a un servidor de logs local.");
        public static string ChkDevLog => T("Enable dev telemetry", "Activar la telemetría de desarrollo");
        public static string LblDevUrl => T("Log server URL", "URL del servidor de logs");
        public static string Disclaimer => T(
            "FeteTimer is a passive notifier; it reads in-game timers and sends alerts. It performs no gameplay automation beyond a user-initiated teleport. Use third-party tools at your own risk.",
            "FeteTimer es un notificador pasivo: lee los temporizadores del juego y te avisa. No automatiza nada del juego salvo un teletransporte que inicias tú. Usa herramientas de terceros bajo tu responsabilidad.");

        // ---- Alerts ----
        public static string AlertTitle => "FeteTimer";
        public static string AlertStartingNow => T("Firmament fête starting now", "La fête del Firmament empieza ahora");
        public static string AlertIn(string countdown) => T($"Firmament fête in {countdown}", $"La fête del Firmament empieza en {countdown}");
        public static string TestAlertMessage => T("Test alert", "Aviso de prueba");

        // ---- Server Info (DTR) bar ----
        public static string DtrNow => T("Fête: now", "Fête: ahora");
        public static string DtrCountdown(string countdown) => $"Fête: {countdown}";
        public static string DtrIdle => "Fête";

        // ---- Countdown ----
        public static string Now => T("now", "ahora");
    }
}
