using System;
using Unity.Netcode;
using UnityEngine;

namespace _Scripts.Stage.Player
{
    public class CollisionBehaviour : NetworkBehaviour
    {
        //     - **OnControllerColliderHit :** CharacterController의 충돌 이벤트 처리 
        //     - `IsServer` 가 true 일 때만 유효 
        //     - **KnockBack :** 플레이어가 충돌체의 역방향으로 짧게 밀려남
        //     - **CancelAction :** 플레이어의 현재 동작 취소
        //     - **Push :** 충돌체가 플레이어의 역방향으로 밀려남 
    
        //     대시 중인 다른 플레이어에 닿았을 경우 + 아이템을 들고 있는데 던져진 아이템이 닿았을 경우
        //     → 넉백 + 하던 동작 취소
    
        //     아이템을 들고 있지 않은데 던져진 아이템이 닿았을 경우
        //     → 아이템 장착 + 하던 동작 취소
        
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            throw new NotImplementedException();
        }
    }
}