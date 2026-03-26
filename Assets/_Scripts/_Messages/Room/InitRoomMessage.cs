using _Scripts.Scene_Stage.Enums;
using Unity.Collections;

namespace _Scripts.Messages.Room
{
    public readonly struct InitRoomMessage
    {
        private readonly FixedString32Bytes _code;

        public string Code => _code.Value;
        public StageMode Mode { get; }
        public int StageIndex { get; }
        public bool IsHostPlayer { get; }

        public InitRoomMessage(string code, StageMode mode, int stageIndex, bool isHostPlayer)
        {
            _code = code;
            Mode = mode;
            StageIndex = stageIndex;
            IsHostPlayer = isHostPlayer;
        }
    }
}