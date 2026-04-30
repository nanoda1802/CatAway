using System.Collections.Generic;

namespace _Scripts.Stage._Data
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