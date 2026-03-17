using _Scripts.Stage.Data;

namespace _Scripts.Messages.Room
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