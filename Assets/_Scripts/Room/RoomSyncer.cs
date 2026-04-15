using _Scripts.Room._Data;
using _Scripts.Room._Messages;
using _Scripts.Shared._Data;
using _Scripts.Stage._Data;
using _Scripts.Stage._Enums;
using MessagePipe;
using Unity.Collections;
using Unity.Netcode;
using VContainer;

namespace _Scripts.Room
{
    public class RoomSyncer : NetworkBehaviour
    {
        private RoomStatus _roomStatus;
        
        private StageListData _stageList;
        
        private readonly NetworkVariable<FixedString32Bytes> _sharedCode = new();
        private readonly NetworkVariable<SelectedStageInfo> _sharedStageInfo = new();

        private IPublisher<InitRoomMessage> _initRoomPub;
        private IPublisher<SwitchModeRespond> _switchModePub;
        private IPublisher<SelectStageRespond> _selectStagePub;
        
        public string RoomCode => _sharedCode.Value.Value;
        public StageMode CurMode => _sharedStageInfo.Value.Mode;
        public int CurStageIndex => _sharedStageInfo.Value.Index;
        
        [Inject]
        private void Construct(
            StageListData stageList,
            RoomStatus roomStatus,
            IPublisher<InitRoomMessage> initRoomPub,
            ISubscriber<SwitchModeRequest> switchModeSub,
            IPublisher<SwitchModeRespond> switchModePub,
            ISubscriber<SelectStageRequest> selectStageSub,
            IPublisher<SelectStageRespond> selectStagePub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _stageList = stageList;
            _roomStatus = roomStatus;
            _initRoomPub = initRoomPub;
            _switchModePub = switchModePub;
            _selectStagePub = selectStagePub;
            
            switchModeSub
                .Subscribe(SwitchStageMode)
                .AddTo(disposableBagBuilder);
            
            selectStageSub
                .Subscribe(SelectStage)
                .AddTo(disposableBagBuilder);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _sharedCode.Value = _roomStatus.Code;
                _sharedStageInfo.Value = _roomStatus.SelectedStage;
            }
            
            var initMsg = new InitRoomMessage(
                    RoomCode,
                    CurMode,
                    CurStageIndex,
                    IsHost
                );
            
            _initRoomPub.Publish(initMsg);
            
            _sharedStageInfo.OnValueChanged += OnSelectionChanged;
            _sharedStageInfo.OnValueChanged += _roomStatus.OnStageChanged;
            
            base.OnNetworkSpawn();
        }
        
        public override void OnNetworkDespawn()
        {
            _sharedStageInfo.OnValueChanged = null;
            
            base.OnNetworkPreDespawn();
        }
        
        private void SwitchStageMode(SwitchModeRequest req)
        {
            if (!IsServer) return;
            
            var newInfo = _sharedStageInfo.Value.SwitchMode().SetStage(0);
            _sharedStageInfo.Value = newInfo;
        }

        private void SelectStage(SelectStageRequest req)
        {
            if (!IsServer) return;
            
            var sideIdx = req.ToLeft 
                ? _stageList.GetLeftIndex(CurMode,CurStageIndex)
                : _stageList.GetRightIndex(CurMode,CurStageIndex);
            
            var newInfo = _sharedStageInfo.Value.SetStage(sideIdx);
            _sharedStageInfo.Value = newInfo;
        }
        
        private void OnSelectionChanged(SelectedStageInfo prev, SelectedStageInfo cur)
        {
            if (cur.IsModeDirty(prev))
            {
                var res = new SwitchModeRespond(cur.Mode);
                _switchModePub.Publish(res);
                return;
            }

            if (cur.IsIndexDirty(prev))
            {
                bool toLeft = cur.Index == _stageList.GetLeftIndex(CurMode, prev.Index);
                var res = new SelectStageRespond(CurMode, cur.Index, toLeft);
                _selectStagePub.Publish(res);
            }
        }
    }
}