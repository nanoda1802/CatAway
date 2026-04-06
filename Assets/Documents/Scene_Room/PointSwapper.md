# PointSwapper.cs 기술 문서

**개요**
`PointSwapper`는 로비 씬에서 `RoomMember`를 드래그 앤 드롭하여 위치를 변경하거나 멤버 간의 순서를 교체할 수 있도록 하는 상호작용 서비스입니다. 호스트 전용으로 동작하며 입력 시스템을 활용합니다.

**필드 (Fields)**
- `_mainCam`: 씬 내 메인 카메라.
- `_netManager`: 네트워크 상태 확인용 `NetworkManager`.
- `_playerInput`: 입력 처리를 위한 `PlayerInput`.
- `_draggingMem`: 현재 드래그 중인 `RoomMember` 객체.
- `_originPoint`: 드래그 시작 시점의 `MemberPoint`.

**주요 메서드 (Methods)**
- **`Initialize` / `Dispose`**: 호스트 여부를 확인하고 입력 액션(`PointerPress`, `PointerPosition`)을 구독/해제합니다.
- **`Detect`**: Raycast를 사용하여 특정 레이어(`MemberPoint`)의 객체를 탐지합니다.
- **`OnPressStarted`**: 드래그 시작 시점의 입력 처리를 수행하며, 대상 멤버를 찾아 드래그 모드로 전환합니다.
- **`OnPressCanceled`**: 드래그 종료 시점의 처리를 수행하며, 드롭된 위치의 `MemberPoint`를 확인하여 위치 이동 또는 멤버 교체(`SwapMem`)를 수행합니다.
- **`OnPosPerformed`**: 드래그 중 멤버의 위치를 입력 위치에 맞춰 갱신합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 객체 및 타입들입니다.*
- **RoomMember (Class)**: 배치 대상인 로비 멤버.
- **MemberPoint (Class)**: 멤버가 위치하는 지점.
- **PlayerInput (Class/Generated)**: 입력 시스템 관련 클래스.
