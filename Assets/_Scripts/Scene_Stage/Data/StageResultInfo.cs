using System.Collections.Generic;
using _Scripts.Scene_Stage.Enums;

namespace _Scripts.Scene_Stage.Data
{
    public struct StageResultInfo
    {
        public readonly List<TeamStatus> ResultByTeam;
        public ulong AcePlayerId { get; }

        public StageResultInfo(ulong aceId, params TeamStatus[] resultByTeam)
        {
            AcePlayerId = aceId;
            ResultByTeam = new List<TeamStatus>(resultByTeam);
        }
    }
}