using System.Collections.Generic;
using System.Linq;
using _Scripts.Messages.Room;
using _Scripts.Scene_Room.Data;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Enums;
using MessagePipe;
using Unity.Collections;
using Unity.Netcode;
using VContainer;

namespace _Scripts.Scene_Room
{
    public class RoomSyncer : NetworkBehaviour
    {
        private RoomStatus _roomStatus;
        
        private StageListData _stageList;
        
        // private readonly Dictionary<ulong, RoomMember> _memberDict = new();
        
        private readonly NetworkVariable<FixedString32Bytes> _sharedCode = new();
        private readonly NetworkVariable<SelectedStageInfo> _sharedStageInfo = new();

        private IPublisher<InitRoomMessage> _initRoomPub;
        // private IPublisher<SwitchStartMessage> _switchStartPub;
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
            // IPublisher<SwitchStartMessage> switchStartPub,
            ISubscriber<SelectStageRequest> selectStageSub,
            IPublisher<SelectStageRespond> selectStagePub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _stageList = stageList;
            _roomStatus = roomStatus;
            _initRoomPub = initRoomPub;
            _switchModePub = switchModePub;
            // _switchStartPub = switchStartPub;
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
        
        // public int InsertMember(ulong clientId, RoomMember newMember)
        // {
        //     var newMemIdx = _roomStatus.InsertMember(clientId, newMember.AvatarIndex);
        //     
        //     _memberDict.Add(clientId, newMember);
        //     
        //     newMember
        //         .AssignTo(this)
        //         .SubscribeAvatarEvent(_roomStatus.Members[newMemIdx]);
        //     
        //     return newMemIdx;
        // }

        // public void RemoveMember(ulong targetId)
        // {
        //     var removed = _roomStatus.RemoveMember(targetId);
        //
        //     if (removed)
        //     {
        //       
        //     }
        //
        //     _memberDict.Remove(targetId);
        // }
        
        // public RoomMember FindMember(ulong targetId)
        // {
        //     return _memberDict[targetId];
        // }

        
        
        private void SwitchStageMode(SwitchModeRequest req)
        {
            if (!IsServer) return;
            
            var newInfo = _sharedStageInfo.Value.SwitchMode().SetStage(0);
            _sharedStageInfo.Value = newInfo;
            
            // InitReadyStates();
            //
            // var msg = new SwitchStartMessage(this.CanStartStage);
            // _switchStartPub.Publish(msg);
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