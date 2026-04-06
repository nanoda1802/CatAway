# MovementBehaviour.cs 기술 문서

**개요**
`MovementBehaviour`는 플레이어의 물리 이동(이동, 회전, 대시)과 관련된 애니메이션 및 시각적 효과(Dash VFX)를 제어하는 `NetworkBehaviour`입니다.

**필드 (Fields)**
- `dashVfx`: 대시 시 재생되는 `ParticleSystem`.
- `_moveStatus`, `_interactStatus`: 이동 및 인터랙션 상태 관리 데이터.
- `_playerRb`: 물리 이동을 위한 `Rigidbody`.
- `_animator`: 이동 관련 애니메이션 제어.
- `_moveAction`, `_dashAction`: 이동 및 대시 입력용 `InputAction`.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입 및 입력 처리를 설정합니다.
- **`NetworkUpdate`**: 고정 업데이트 루프에서 플레이어의 이동(`Move`)과 회전(`Rotate`) 처리를 수행합니다.
- **`Dash`**: 비동기 대시 로직을 처리하며 물리적인 힘을 적용합니다.
- **`OnMovePerformed` / `OnDashStarted`**: 입력 발생 시 상태를 업데이트하고 애니메이션과 효과를 제어합니다.
- **`ActivateVfxRpc` / `DeactivateVfxRpc`**: 대시 이펙트를 RPC를 통해 네트워크 전체에 동기화합니다.
