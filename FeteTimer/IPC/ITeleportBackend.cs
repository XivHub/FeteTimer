namespace FeteTimer.IPC
{
    public interface ITeleportBackend
    {
        bool Installed { get; }
        bool TryTeleportToFirmament();
        string Name { get; }
    }
}
