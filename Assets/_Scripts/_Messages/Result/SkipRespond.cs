namespace _Scripts.Messages.StageResult
{
    public readonly struct SkipRespond
    {
        public int Agreements { get; }
        public int VoterCount { get; }

        public SkipRespond(int agreements, int voterCount)
        {
            Agreements = agreements;
            VoterCount = voterCount;
        }
    }
}