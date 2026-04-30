using System;
using _Scripts.Stage._Enums;
using Unity.Netcode;

namespace _Scripts.Room._Data
{
    public struct SelectedStageInfo : INetworkSerializable, IEquatable<SelectedStageInfo>
    {
        private StageMode _mode;
        private int _index;

        public StageMode Mode => _mode;
        public int Index => _index;
        
        public SelectedStageInfo SwitchMode()
        {
            _mode =  _mode switch
            {
                StageMode.Coop => StageMode.Comp,
                StageMode.Comp => StageMode.Coop,
                _ => throw new Exception("[StageSelection.SwitchMode] 구현되지 않은 타입의 Mode입니다.")
            };
            
            return this;
        }

        public SelectedStageInfo SetStage(int stageIdx)
        {
            _index = stageIdx;
            return this;
        }

        public bool IsModeDirty(SelectedStageInfo prev)
        {
            return _mode != prev.Mode;
        }

        public bool IsIndexDirty(SelectedStageInfo prev)
        {
            return _index != prev.Index;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _mode);
            serializer.SerializeValue(ref _index);
        }

        public bool Equals(SelectedStageInfo other)
        {
            return !this.IsIndexDirty(other) && !this.IsModeDirty(other);
        }
    }
}