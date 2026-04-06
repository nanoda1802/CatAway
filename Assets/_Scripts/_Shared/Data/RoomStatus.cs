using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Enums;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using VContainer.Unity;

namespace _Scripts.Scene_Room.Data
{
    public class RoomStatus : IInitializable, IDisposable
    {
        private readonly NetworkManager _netManager;
        private readonly StageListData _stageList;
        
        public readonly MemberInfo[] Members = new MemberInfo[4];
        public string Code;
        public SelectedStageInfo SelectedStage;
        private StageResultInfo _stageResult;

        public bool IsFull => Members.All(mem => mem != null);
        public IEnumerable<MemberInfo> ActiveMembers => Members.Where(mem => mem != null);
        public StageData CurStageData => _stageList.GetStageData(SelectedStage.Mode, SelectedStage.Index);
        public StageResultInfo StageResult => _stageResult;
        public bool EachTeamHasMember
        {
            get
            {
                return SelectedStage.Mode switch
                {
                    StageMode.Comp => (Members[0] != null || Members[1] != null) 
                                      && (Members[2] != null || Members[3] != null),
                    StageMode.Coop => ActiveMembers.Any(),
                    _ => false
                };
            }
        }
        
        public RoomStatus(
            NetworkManager netManager,
            StageListData stageList)
        {
            _netManager = netManager;
            _stageList = stageList;

            _netManager.OnPreShutdown += Initialize;
        }

        public void Initialize()
        {
            Array.Clear(Members, 0, Members.Length);
            Code = string.Empty;
            SelectedStage = default;
        }

        public void Dispose()
        {
            _netManager.OnPreShutdown -= Initialize;
        }

        public void OnCodeChanged(FixedString32Bytes prev, FixedString32Bytes cur)
        {
            if (prev == cur) return;
            Code = cur.Value;
        }

        public void OnStageChanged(SelectedStageInfo prev, SelectedStageInfo cur)
        {
            if (prev.Equals(cur)) return;
            SelectedStage = cur;

            if (cur.IsModeDirty(prev))
            {
                for (int i = 0; i < Members.Length; i++)
                {
                    Members[i]?.Apply(SetTeamByIndex(i));
                }
            }
        }

        public MemberInfo GetMemberById(ulong id)
        {
            for (int i = 0; i < Members.Length; i++)
            {
                if (Members[i] == null) continue;
                if (Members[i].ClientId == id) return Members[i];
            }
            
            return null;
        }

        public ulong? GetIdByIndex(int idx)
        {
            return Members[idx]?.ClientId;
        }

        public int? GetIndexById(ulong clientId)
        {
            for (int i = 0; i < Members.Length; i++)
            {
                if (Members[i] == null) continue;
                if (Members[i].ClientId == clientId) return i;
            }
            
            return null;
        }

        public void Report(string type)
        {
            Debug.Log($"<color=red>- - - - - - - - [Report_{type}] - - - - - - - -</color> {Time.realtimeSinceStartup}");
            for (int i = 0; i < Members.Length; i++)
            {
                var info = Members[i];
                Debug.Log($"[no.{i}] memInfo? {info != null} / id? {info?.ClientId} / team? {info?.Team} / {Time.realtimeSinceStartup}");
            }
            Debug.Log($"<color=red>- - - - - - - - - - - - - - - - - - - - - - - - -</color> {Time.realtimeSinceStartup}");
        }

        public int InsertMember(ulong memberId)
        {
            var newMemIdx = Array.FindIndex(Members, mem => mem == null);
            if (newMemIdx < 0 || newMemIdx >= Members.Length) return -1;
            
            Members[newMemIdx] = new MemberInfo(memberId, SetTeamByIndex(newMemIdx));
            Report("Insert");
            return newMemIdx;
        }
        
        public bool RemoveMember(ulong targetId, out int idx)
        {
            idx = -1;
            
            for (int i = 0; i < Members.Length; i++)
            {
                if (Members[i] == null || Members[i].ClientId != targetId) continue;
                
                Members[i] = null;
                idx = i;
                Report("Remove");
                return true;
            }
            
            return false;
        }
        
        public bool SwapMember(int idx1, int idx2)
        {
            if (idx1 == idx2) return false;
            if (idx1 < 0 || idx1 >= Members.Length) return false;
            if (idx2 < 0 || idx2 >= Members.Length) return false;
            
            var mem1 = Members[idx1];
            var mem2 = Members[idx2];
            
            if (mem1 == null && mem2 == null) return false;
            
            (Members[idx1], Members[idx2]) = (mem2, mem1);
            
            mem1?.Apply(SetTeamByIndex(idx2));
            mem2?.Apply(SetTeamByIndex(idx1));
            
            return true;
        }

        private Team SetTeamByIndex(int idx)
        {
            if (idx is < 0 or >= 4) return Team.None;
            if (SelectedStage.Mode == StageMode.Coop) return Team.None;

            return idx < 2 ? Team.Blue : Team.Red;
        }

        public void RecordStageResult(ulong aceId, params TeamStatus[] resultByTeam)
        {
            Debug.Log($"<color=cyan>[RoomStatus]</color> record : ace? {aceId} / resultByTeam? {resultByTeam != null}");
            _stageResult = new StageResultInfo(aceId, resultByTeam);
        }
    }
}