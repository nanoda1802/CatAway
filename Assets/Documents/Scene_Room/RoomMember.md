# RoomMember.cs 기술 문서

**개요**
`RoomMember`는 로비 씬에서 개별 플레이어의 상태를 네트워크상에서 동기화하고, 관련 UI(카드 및 상태)를 관리하는 `NetworkBehaviour`입니다.

**필드 (Fields)**
- `_sharedAvatarIndex`, `_sharedReadyState`, `_sharedNickname`: 네트워크 동기화 변수들(`NetworkVariable`).
- `_netTr`, `_renderer`, `_animator`: 네트워크 트랜스폼 및 시각적 컴포넌트.
- `_memberSyncer`: 멤버 관리자(`RoomMemberSyncer`) 참조.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 데이터와 UI 메시지 발행기들을 초기화하고, 외부 메시지 구독을 설정합니다.
- **`OnNetworkSpawn` / `OnNetworkDespawn`**: 네트워크 스폰 시 초기 상태를 설정하고, 변경 이벤트 등록/해제를 수행합니다.
- **`OnNetworkPostSpawn`**: 스폰 완료 후 UI 카드 표시(`ShowCard`) 및 초기 아바타 설정.
- **`SetNickname` / `SetAvatar` / `SetReadyState`**: 오너 클라이언트에서 로컬 입력에 따라 네트워크 동기화 변수를 갱신합니다.
- **`StartDrag` / `MoveTo`**: 멤버의 위치를 드래그하거나 이동할 때 애니메이션을 제어하고, 필요시 카드 위치를 갱신(`MoveCardRpc`)합니다.
- **`ShowCard` / `HideCardRpc` / `MoveCardRpc`**: UI 카드 정보를 네트워크의 모든 클라이언트에 발행합니다.
- **`InitReadyStateRpc`**: 호스트 전용으로 레디 상태를 초기화하는 RPC입니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 및 타입들입니다.*
- **AvatarData, PlayerStatus (Class/ScriptableObject)**: 아바타 및 플레이어 데이터 관리.
- **MemberIconType (Enum)**: 멤버 아이콘 상태(Host, Ready, NonReady).
- **다양한 메시지 클래스 (Struct/Class)**: UI 및 네트워크 통신용 메시지.
