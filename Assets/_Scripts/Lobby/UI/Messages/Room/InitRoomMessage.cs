using _Scripts.Stage;
using Unity.Collections;

namespace _Scripts.Lobby.UI.Messages.Room
{
    public readonly struct InitRoomMessage
    {
        private readonly FixedString32Bytes _code;
        private readonly StageMode _mode;
        private readonly bool _isHostPlayer;
        
        public string Code => _code.Value;
        public StageMode Mode => _mode;
        public bool IsHostPlayer => _isHostPlayer;

        public InitRoomMessage(string code, StageMode mode, bool isHostPlayer)
        {
            _code = code;
            _mode = mode;
            _isHostPlayer = isHostPlayer;
        }
    }
}