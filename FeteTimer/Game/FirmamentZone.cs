using System;

namespace FeteTimer.Game
{
    /// <summary>
    /// Static facts about the Firmament (Ishgardian Restoration) zone.
    /// </summary>
    public static class FirmamentZone
    {
        /// <summary>
        /// TerritoryType ids treated as "the Firmament".
        /// 886 = Firmament / Ishgardian Restoration (confirmed datamining value; verify in-game once — V1).
        /// The Diadem (939) is intentionally excluded — it is not where the Skyrise Celebration fête runs.
        /// </summary>
        public static readonly ushort[] TerritoryTypeIds = { 886 };

        /// <summary>
        /// Aetheryte id used by the "Teleport to Firmament" button.
        /// There is no dedicated Firmament aetheryte — teleport to Foundation/Ishgard (70) then aethernet in.
        /// Confirm-once — V5.
        /// </summary>
        public const uint FoundationAetheryteId = 70;

        /// <summary>True when the given TerritoryType id is one of the Firmament zones.</summary>
        public static bool IsInFirmament(ushort territory) => Array.IndexOf(TerritoryTypeIds, territory) >= 0;
    }
}
