using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Shared._Data;
using _Scripts.Stage._Enums;
using _Scripts.Stage._Messages;
using MessagePipe;

namespace _Scripts.Stage._Data
{
    public class StageStatus
    {
        private readonly RoomStatus _room;
        
        private readonly Dictionary<Team, TeamStatus> _statusByTeam;
        private readonly Dictionary<ulong, int> _pointsByPlayer;

        public ulong AcePlayerId
        {
            get
            {
                var maxPoint = int.MinValue;
                var acePlayerId = ulong.MinValue;
            
                foreach (var pair in _pointsByPlayer)
                {
                    if (pair.Value <= maxPoint) continue; 
                
                    maxPoint = pair.Value;
                    acePlayerId = pair.Key;
                }
            
                return acePlayerId;
            }
        }

        public StageStatus(
            RoomStatus room,
            StageData stageData,
            ISubscriber<EndStageMessage> endSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _room = room;
            
            _statusByTeam = new Dictionary<Team, TeamStatus>();
            _pointsByPlayer = new Dictionary<ulong, int>();
            
            switch (stageData.Mode)
            {
                case StageMode.Coop:
                    _statusByTeam.Add(Team.None, new TeamStatus(Team.None));
                    break;
                case StageMode.Comp:
                    _statusByTeam.Add(Team.Blue, new TeamStatus(Team.Blue));
                    _statusByTeam.Add(Team.Red, new TeamStatus(Team.Red));
                    break;
                default:    
                    throw new ArgumentOutOfRangeException();
            }

            endSub
                .Subscribe(RecordResult)
                .AddTo(disposableBagBuilder);
        }

        public void RecordTotalOrderCount(Team team)
        {
            _statusByTeam[team].TotalOrderCount += 1;
        }

        public (int,int) RecordCurScore(Team team, int point, ulong scorerId)
        {
            if (!_pointsByPlayer.TryAdd(scorerId, point)) _pointsByPlayer[scorerId] += point;
            
            _statusByTeam[team].CurScore = point;
            _statusByTeam[team].CurCombo = point;
            
            return (_statusByTeam[team].CurScore, _statusByTeam[team].CurCombo);
        }

        public int GetTotalScore(Team team)
        {
            return _statusByTeam[team].CurScore;
        }

        public int GetCurCombo(Team team)
        {
            return _statusByTeam[team].CurCombo;
        }

        public int GetBestCombo(Team team)
        {
            return _statusByTeam[team].BestCombo;
        }

        public int GetDeliverPercentage(Team team)
        {
            return (int) (_statusByTeam[team].DeliverRatio * 100);
        }

        private void RecordResult(EndStageMessage msg)
        {
            _room.RecordStageResult(AcePlayerId, _statusByTeam.Values.ToArray());
        }
    }
}