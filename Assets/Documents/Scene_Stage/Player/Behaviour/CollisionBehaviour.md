# CollisionBehaviour.cs 기술 문서

**개요**
`CollisionBehaviour`는 플레이어가 투척된 아이템과 충돌했을 때, 상태에 따라 아이템을 받거나(Pick) 플레이어가 넉백(KnockBack)되는 상호작용을 처리하는 `NetworkBehaviour`입니다.

**필드 (Fields)**
- `_moveStatus`: 플레이어의 이동 제약 및 넉백 파라미터 관리 데이터.
- `_carrierBehaviour`: 아이템 운반 및 부착 상태를 확인하기 위한 `CarrierBehaviour` 참조.
- `_playerRb`: 플레이어 이동 처리를 위한 `Rigidbody`.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 서비스들을 초기화하고 아이템 태그를 캐싱합니다.
- **`OnCollisionEnter`**: 투척된 아이템과의 충돌을 감지합니다. 이미 아이템을 들고 있다면 `KnockBackClientRpc`를 호출하여 플레이어를 넉백시키고, 빈 손이라면 아이템을 자동으로 받습니다.
- **`KnockBackClientRpc`**: 플레이어에게 넉백 명령을 전달하는 RPC입니다.
- **`KnockBack`**: 플레이어의 움직임을 제한하고 물리적인 충격량을 적용하여 넉백 효과를 구현하는 비동기 메서드입니다.
