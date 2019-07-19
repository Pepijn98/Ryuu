// ReSharper disable UnusedMemberInSuper.Global

namespace Ryuu.Interfaces
{
    public interface IBlacklist
    {
        string Username { get; set; }
        string Discriminator { get; set; }
        string Reason { get; set; }
    }
}