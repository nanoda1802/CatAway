using _Scripts.Stage;

namespace _Scripts.Lobby.UI.Messages.Room
{
    public readonly struct SelectStageRespond
    {
        public StageMode CurMode { get; }
        public int CurStageIndex { get; }
        public bool ToLeft { get; }

        public SelectStageRespond(StageMode mode, int curStageIndex, bool toLeft)
        {
            CurMode = mode;
            CurStageIndex = curStageIndex;
            ToLeft = toLeft;
        }
    }
}