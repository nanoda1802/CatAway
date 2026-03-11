using System.Threading;

namespace _Scripts.Lobby.UI.Messages.Room
{
    public readonly struct JoinRoomRequest
    {
        public string Code { get; }
        public CancellationToken Ct { get; }

        public JoinRoomRequest(string code, CancellationToken ct)
        {
            Code = code;
            Ct = ct;
        }
    }
}