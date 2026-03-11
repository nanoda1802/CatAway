using System.Threading;

namespace _Scripts.Lobby.UI.Messages.Room
{
    public readonly struct CreateRoomRequest
    {
        public CancellationToken Ct { get; }

        public CreateRoomRequest(CancellationToken ct)
        {
            Ct = ct;
        }
    }
}