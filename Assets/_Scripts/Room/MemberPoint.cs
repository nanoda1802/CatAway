using System;
using UnityEngine;

namespace _Scripts.Room
{
    public class MemberPoint : MonoBehaviour
    {
        private RoomMember _curMem;
        
        public int PointIdx { get; private set; } = -1;
        public RoomMember CurMem => _curMem;
        public bool HasMem => CurMem != null;
        public Vector3 Pos => transform.position;
        public Quaternion Rot => transform.rotation;

        public event Action<int, int> OnSwap;

        public void Init(int idx, Action<int, int> onSwap)
        {
            PointIdx = idx;
            OnSwap += onSwap;
        }

        public void Assign(RoomMember member)
        {
            _curMem = member;
        }

        public void Resign()
        {
            _curMem = null;
        }

        public void SwapMem(MemberPoint other)
        {
            if (other == this)
            {
                _curMem.MoveTo(other.Pos,other.Rot);
                return;
            }
            
            RoomMember thisMem = CurMem;

            if (!other.HasMem)
            {
                this.Resign();
                
                other.Assign(thisMem);   
                thisMem.MoveTo(other.Pos,other.Rot);    
            }
            else if (other.HasMem)
            {
                Assign(other.CurMem);
                other.CurMem.MoveTo(this.Pos,this.Rot);
            
                other.Assign(thisMem);
                thisMem.MoveTo(other.Pos,other.Rot); 
            }
            
            OnSwap?.Invoke(this.PointIdx, other.PointIdx);
        }
    }
}