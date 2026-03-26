using UnityEngine;

namespace _Scripts._Messages.Shared
{
    public readonly struct MoveMemberCardMessage
    {
        public ulong MemberId { get; }
        public Vector3 NewPos { get; }
        public Quaternion NewRot { get; }

        public MoveMemberCardMessage(ulong memberId, Vector3 newPos, Quaternion newRot)
        {
            MemberId = memberId;
            NewPos = newPos;
            NewRot = newRot;
        }
    }
}