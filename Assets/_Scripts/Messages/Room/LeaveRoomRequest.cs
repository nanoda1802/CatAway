namespace _Scripts.Messages.Room
{
    public readonly struct LeaveRoomRequest
    {
        public ulong ClientId { get; }

        public LeaveRoomRequest(ulong clientId)
        {
            ClientId = clientId;
        }
    }
}