// ReSharper disable UnusedMemberInSuper.Global

namespace Ryuu.Interfaces
{
    public interface IServer
    {
        ulong JoinLeave { get; set; }
        ulong ModLog { get; set; }
        ulong ModRole { get; set; }
        string CommandPrefix { get; set; }
    }
}