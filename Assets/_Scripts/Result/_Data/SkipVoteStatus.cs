using System;
using Unity.Netcode;

namespace _Scripts.Result._Data
{
    public struct SkipVoteStatus : INetworkSerializable, IEquatable<SkipVoteStatus>
    {
        public int Agreements;
        public int VoterCount;
        
        public bool AllAgreed => Agreements == VoterCount;

        public SkipVoteStatus(int agreements, int voterCount)
        {
            this.Agreements = agreements;
            this.VoterCount = voterCount;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref VoterCount);
            serializer.SerializeValue(ref Agreements);
        }

        public bool Equals(SkipVoteStatus other)
        {
            return Agreements == other.Agreements && VoterCount == other.VoterCount;
        }
    }
}