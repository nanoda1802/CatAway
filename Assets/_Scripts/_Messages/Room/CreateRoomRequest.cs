using System.Threading;

namespace _Scripts.Messages.Room
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