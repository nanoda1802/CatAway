# MoveStatus.cs 기술 문서

**개요**
`MoveStatus`는 플레이어의 이동 속도, 대시, 넉백 상태 및 이동 제약과 같은 물리적 상태 정보를 관리하는 서비스입니다.

**필드 (Fields)**
- `_moveDir`: 현재 이동 방향.
- `_speedMultiplier`: 현재 속도 배율.
- `MoveConstraint`: 이동 가능 여부를 제어하는 플래그.

**주요 메서드 (Methods)**
- **`SetMoveDirection(Vector2)`**: 입력 벡터를 3D 이동 방향으로 변환합니다.
- **`UpdateSpeedMultiplier(bool)`**: 대시 상태에 따라 이동 속도 배율을 최신화합니다.
- **`UpdateLastDashTime`**: 마지막 대시 수행 시각을 갱신합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 객체입니다.*
- **PlayerData (ScriptableObject)**: 플레이어 물리/이동 설정 데이터.
