using System;
using _Scripts.Stage;
using Unity.Netcode;

namespace _Scripts.Lobby.Room
{
    public struct StageSelection : INetworkSerializable, IEquatable<StageSelection>
    {
        private StageMode _selectedMode;
        private int _selectedStageIndex;

        public StageMode SelectedMode => _selectedMode;
        public int SelectedStageIndex => _selectedStageIndex;
        
        public StageSelection SwitchMode()
        {
            _selectedMode =  _selectedMode switch
            {
                StageMode.Coop => StageMode.Comp,
                StageMode.Comp => StageMode.Coop,
                _ => throw new Exception("[StageSelection.SwitchMode] 구현되지 않은 타입의 Mode입니다.")
            };
            
            return this;
        }

        public StageSelection SetStage(int stageIdx)
        {
            _selectedStageIndex = stageIdx;
            return this;
        }

        public bool IsModeDirty(StageSelection prev)
        {
            return _selectedMode != prev.SelectedMode;
        }

        public bool IsIndexDirty(StageSelection prev)
        {
            return _selectedStageIndex != prev.SelectedStageIndex;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _selectedMode);
            serializer.SerializeValue(ref _selectedStageIndex);
        }

        public bool Equals(StageSelection other)
        {
            return !this.IsIndexDirty(other) && !this.IsModeDirty(other);
        }
    }
}