using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Lobby.UI.Messages.Room;
using _Scripts.Stage;
using _Scripts.Stage.Data;
using MessagePipe;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

namespace _Scripts.Lobby.Room
{
    public class RoomSyncer : NetworkBehaviour
    {
        private readonly RoomMember[] _members = new RoomMember[4];

        private StageListData _stageList;
        private IPublisher<SwitchStartMessage> _switchStartPub;
        private IPublisher<SwitchModeRespond> _switchModePub;
        private IPublisher<SelectStageRespond> _selectStagePub;
        
        private readonly NetworkVariable<StageSelection> _sharedStageSelection = new();
        
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
        public bool IsFull => _members.All(mem => mem != null);
        public IEnumerable<RoomMember> ActiveMembers => _members.Where(mem => mem != null);
        public StageMode CurMode => _sharedStageSelection.Value.SelectedMode;
        public int CurStageIndex => _sharedStageSelection.Value.SelectedStageIndex;
        public bool CanStartStage
        {
            get
            {
                if (!IsServer) return false;
                
                if (CurMode == StageMode.Comp)
                {
                    var eachTeamHasMember =
                        (_members[0] != null || _members[1] != null)
                        && (_members[2] != null || _members[3] != null);

                    if (!eachTeamHasMember) return false;
                }
                
                foreach (var mem in _members)
                {
                    if (mem is null) continue;
                    if (!mem.IsReady) return false;
                }
            
                return true;
            }
        }
        
        [Inject]
        private void Construct(
            StageListData stageList,
            IPublisher<SwitchStartMessage> switchStartPub,
            IPublisher<SelectStageRespond> selectStagePub,
            ISubscriber<SelectStageRequest> selectStageSub,
            IPublisher<SwitchModeRespond> switchModePub,
            ISubscriber<SwitchModeRequest> switchModeSub)
        {
            _stageList = stageList;
            _switchStartPub = switchStartPub;
            _selectStagePub = selectStagePub;
            _switchModePub = switchModePub;
            
            selectStageSub
                .Subscribe(SelectStage)
                .AddTo(_disposableBagBuilder);
            
            switchModeSub
                .Subscribe(SwitchStageMode)
                .AddTo(_disposableBagBuilder);
        }

        public override void OnNetworkSpawn()
        {
            _sharedStageSelection.OnValueChanged = OnSelectionChanged;
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            _sharedStageSelection.OnValueChanged = null;
            
            Array.Clear(_members, 0, _members.Length);
            
            base.OnNetworkPreDespawn();
        }

        public override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            
            base.OnDestroy();
        }

        public void InitStageSelection()
        {
            _sharedStageSelection.Value = default;
        }

        private void ReportStatus()
        {
            Debug.Log($"- - - - - Room status - - - - -");
            for (int i = 0; i < _members.Length; i++)
            {
                var mem = _members[i];
                Debug.Log($"[{i}번] null? {mem==null} / ownerId? {mem?.OwnerClientId} / spawn? {mem?.IsSpawned} / destroy? {mem?.IsDestroyed()}");
            }
            Debug.Log($"- - - - - - - - - - - - - - - -");
        }
        
        public int InsertMember(RoomMember newMember)
        {
            var newMemIdx = Array.FindIndex(_members, mem => mem == null);
            if (newMemIdx < 0 || newMemIdx >= _members.Length) return -1;
            
            _members[newMemIdx] = newMember.AssignTo(this);

            var msg = new SwitchStartMessage(CurMode == StageMode.Coop && newMemIdx == 0);
            _switchStartPub.Publish(msg);
            
            return newMemIdx;
        }

        public void RemoveMember(ulong targetId)
        {
            for (int i = 0; i < _members.Length; i++)
            {
                if (_members[i] == null || _members[i].OwnerClientId != targetId) continue;
                
                _members[i] = null;
                
                var msg = new SwitchStartMessage(this.CanStartStage);
                _switchStartPub.Publish(msg);
                
                return;
            }
        }

        public RoomMember FindMember(ulong targetId)
        {
            return Array.Find(_members, mem => mem != null && mem.OwnerClientId == targetId);
        }

        public bool SwapMember(int idx1, int idx2)
        {
            if (idx1 == idx2) return false;
            if (idx1 < 0 || idx1 >= _members.Length) return false;
            if (idx2 < 0 || idx2 >= _members.Length) return false;
            
            var mem1 = _members[idx1];
            var mem2 = _members[idx2];
            
            if (mem1 == null && mem2 == null) return false;
            
            (_members[idx1], _members[idx2]) = (mem2, mem1);
            
            mem1?.InitReadyStateRpc();
            mem2?.InitReadyStateRpc();
            
            return true;
        }
        
        private void SwitchStageMode(SwitchModeRequest req)
        {
            if (!IsServer) return;
            
            var newSelection = _sharedStageSelection.Value.SwitchMode().SetStage(0);
            _sharedStageSelection.Value = newSelection;
            
            InitReadyStates();
            
            var msg = new SwitchStartMessage(this.CanStartStage);
            _switchStartPub.Publish(msg);
        }

        private void SelectStage(SelectStageRequest req)
        {
            if (!IsServer) return;
            
            var sideIdx = req.ToLeft 
                ? _stageList.GetLeftIndex(CurMode,CurStageIndex)
                :  _stageList.GetRightIndex(CurMode,CurStageIndex);
            
            var newSelection = _sharedStageSelection.Value.SetStage(sideIdx);
            _sharedStageSelection.Value = newSelection;
        }

        private void InitReadyStates()
        {
            foreach (var mem in _members)
            {
                if (mem == null || mem.IsHostMember) continue;
                mem.InitReadyStateRpc();
            }
        }

        private void OnSelectionChanged(StageSelection prev, StageSelection cur)
        {
            if (cur.IsModeDirty(prev))
            {
                var res = new SwitchModeRespond(cur.SelectedMode);
                _switchModePub.Publish(res);
                return;
            }

            if (cur.IsIndexDirty(prev))
            {
                bool toLeft = cur.SelectedStageIndex == _stageList.GetLeftIndex(CurMode, prev.SelectedStageIndex);
                var res = new SelectStageRespond(CurMode, cur.SelectedStageIndex, toLeft);
                _selectStagePub.Publish(res);
            }
        }
    }
}