namespace FeteTimer.IPC
{
    /// <summary>
    /// Prefers Lifestream if installed, else Teleporter if installed, else no-ops with a chat hint.
    /// </summary>
    public class CompositeTeleportBackend : ITeleportBackend
    {
        private readonly ITeleportBackend lifestream;
        private readonly ITeleportBackend teleporter;

        public CompositeTeleportBackend(ITeleportBackend lifestream, ITeleportBackend teleporter)
        {
            this.lifestream = lifestream;
            this.teleporter = teleporter;
        }

        public bool Installed => lifestream.Installed || teleporter.Installed;

        public string Name
        {
            get
            {
                if (lifestream.Installed) return lifestream.Name;
                if (teleporter.Installed) return teleporter.Name;
                return "none";
            }
        }

        public bool TryTeleportToFirmament()
        {
            if (lifestream.Installed)
                return lifestream.TryTeleportToFirmament();

            if (teleporter.Installed)
                return teleporter.TryTeleportToFirmament();

            Plugin.ChatGui.Print("[FeteTimer] Install Teleporter or Lifestream to use the teleport button.");
            return false;
        }
    }
}
