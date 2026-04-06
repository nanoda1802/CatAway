# InteractionBehaviour.cs 기술 문서

**개요**
`InteractionBehaviour`는 플레이어가 테이블 등 상호작용 가능한 객체와 상호작용할 때 애니메이션과 제약을 관리하는 `NetworkBehaviour`입니다.

**필드 (Fields)**
- `_detectStatus`, `_interactStatus`, `_carryStatus`, `_moveStatus`: 플레이어 및 상호작용 관련 상태 객체.
- `_playerRb`: 이동 제약을 위한 Rigidbody.
- `_animator`: 상호작용 애니메이션 재생을 위한 Animator.
- `_interactAction`: 인터랙션 입력용 `InputAction`.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 서비스 및 입력 이벤트를 초기화합니다.
- **`TryInteractRpc`**: 테이블 탐지 및 상호작용 가능 여부를 확인하고 상호작용을 시작합니다.
- **`CancelRpc` / `FinishRpc`**: 진행 중인 상호작용을 취소하거나 완료하고 상태를 복원합니다.
- **`StartInteractionRpc` / `StopInteractionRpc`**: 서버 및 클라이언트에서 상호작용 시 움직임 제약(Rigidbody.constraints) 및 애니메이션을 시작/정지합니다.
- **`SubscribeInputEvents` / `UnsubscribeInputEvents`**: 스테이지 시작/종료 시 입력 이벤트를 관리합니다.
