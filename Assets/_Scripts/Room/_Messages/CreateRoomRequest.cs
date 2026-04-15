using System.Threading;

namespace _Scripts.Room._Messages
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