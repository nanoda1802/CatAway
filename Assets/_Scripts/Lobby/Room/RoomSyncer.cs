using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Lobby.UI.Messages.Room;
using _Scripts.Stage;
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
        
        private readonly NetworkVariable<StageMode> _sharedStageMode = new(StageMode.Coop);

        private IPublisher<SwitchStartMessage> _switchStartPub;
        private IPublisher<SwitchModeRespond> _switchModePub;
        
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
        public bool IsFull => _members.All(mem => mem != null);
        public IEnumerable<RoomMember> ActiveMembers => _members.Where(mem => mem != null);
        public StageMode CurMode => _sharedStageMode.Value;
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
            IPublisher<SwitchStartMessage> switchStartPub,
            IPublisher<SwitchModeRespond> switchModePub,
            ISubscriber<SwitchModeRequest> switchModeSub)
        {
            _switchStartPub = switchStartPub;
            _switchModePub = switchModePub;
            
            switchModeSub
                .Subscribe(SwitchStageMode)
                .AddTo(_disposableBagBuilder);
        }

        public override void OnNetworkSpawn()
        {
            _sharedStageMode.OnValueChanged = OnModeChanged;
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkPreDespawn()
        {
            _sharedStageMode.OnValueChanged = null;
            
            Array.Clear(_members, 0, _members.Length);
            
            base.OnNetworkPreDespawn();
        }

        public override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            
            base.OnDestroy();
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

            ReportStatus();
            _switchStartPub.Publish(new SwitchStartMessage(CurMode == StageMode.Coop && newMemIdx == 0));
            
            return newMemIdx;
        }

        public void RemoveMember(ulong targetId)
        {
            for (int i = 0; i < _members.Length; i++)
            {
                if (_members[i] == null || _members[i].OwnerClientId != targetId) continue;
                
                _members[i] = null;
                
                ReportStatus();
                _switchStartPub.Publish(new SwitchStartMessage(CanStartStage));
                
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
            
            _sharedStageMode.Value = CurMode switch
            {
                StageMode.Coop => StageMode.Comp,
                StageMode.Comp => StageMode.Coop,
                _ => throw new Exception("[Room.SwitchMode] 구현되지 않은 타입의 Mode입니다.")
            };

            InitReadyStates();
            
            _switchStartPub.Publish(new SwitchStartMessage(CanStartStage));
        }

        private void InitReadyStates()
        {
            foreach (var mem in _members)
            {
                if (mem == null || mem.IsHostMember) continue;
                mem.InitReadyStateRpc();
            }
        }

        private void OnModeChanged(StageMode prevMode, StageMode newMode)
        {
            if (prevMode == newMode) return;
            
            var msg = new SwitchModeRespond(newMode);
            _switchModePub.Publish(msg);
        }
    }
}