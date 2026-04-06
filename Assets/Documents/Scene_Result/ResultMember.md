# ResultMember.cs 기술 문서

**개요**
`ResultMember`는 결과(Result) 씬에서 각 플레이어의 데이터를 네트워크상에서 동기화하고 시각적 UI 카드와 연결하는 `NetworkBehaviour`입니다.

**필드 (Fields)**
- `_netManager`, `_renderer`: 네트워크 트랜스폼 및 렌더러 컴포넌트.
- `_sharedAvatarIndex`, `_sharedNickname`, `_sharedTeam`, `_sharedAceId`: 네트워크 동기화 변수들(`NetworkVariable`).
- `_showCardPub`, `_hideCardPub`, 등: UI 카드와의 상호작용을 위한 메시지 발행기들.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입 및 아바타 변경 메시지 구독 설정.
- **`OnNetworkSpawn` / `OnNetworkDespawn`**: 네트워크 스폰 시 플레이어 데이터(아바타 인덱스, 팀, 에이스 ID)를 초기화하고 동기화 변수 변경 이벤트를 등록/해제합니다.
- **`OnNetworkPostSpawn`**: 스폰 직후 UI 카드 표시(`ShowCard`) 및 초기 아바타를 적용합니다.
- **`RePosition`**: 서버에서 플레이어 위치를 갱신하고 연관된 UI 카드의 위치도 함께 이동(`MoveCardRpc`)시킵니다.
- **`SetAvatar` / `OnAvatarIndexChanged`**: 로컬/네트워크 상의 아바타 변경 사항을 감지하고 모델 외형을 갱신합니다.
- **`ShowCard` / `HideCardRpc` / `MoveCardRpc`**: UI 카드 정보를 네트워크의 모든 클라이언트에 발행합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 객체 및 타입들입니다.*
- **RoomStatus, AvatarData, PlayerStatus (Class/ScriptableObject)**: 플레이어 정보 관리.
- **Team (Enum)**: 소속 팀 정보.
- **다양한 메시지 클래스 (Struct/Class)**: UI 카드와의 통신 메시지.
