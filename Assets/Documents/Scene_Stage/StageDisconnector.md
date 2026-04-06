# StageDisconnector.cs 기술 문서

**개요**
`StageDisconnector`는 스테이지 씬에서 게임이 종료되거나 플레이어가 나갈 때 네트워크를 정리하고 사운드를 멈추며 홈 씬으로 전환하는 `NetworkBehaviour`입니다.

**필드 (Fields)**
- `_roomStatus`: 방의 상태(`RoomStatus`) 관리 객체.
- `_soundManager`: BGM 및 SFX 제어를 위한 사운드 관리자.
- `_loadScenePub` 등: UI 알림 및 씬 전환을 위한 메시지 발행기들.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 사운드 매니저와 메시지 발행기들을 초기화하고, 스테이지 종료(`EndStageMessage`) 및 방 나가기(`LeaveRoomMessage`) 메시지를 구독합니다.
- **`OnNetworkSpawn` / `OnNetworkDespawn`**: 네트워크 연결 이벤트를 구독/해제합니다.
- **`LeaveRoom`**: 방 나가기 요청 처리. 스테이지 소리를 멈추고 서버/클라이언트 상황에 맞게 네트워크를 종료한 후 홈 씬으로 전환합니다.
- **`StopSounds`**: 게임 종료 시 BGM 재생을 멈춥니다.
- **`OnConnection`**: 연결 해제 시 처리. 서버 환경이면 방 상태에서 멤버를 제거하고, 로컬 클라이언트 연결 해제 시 다이얼로그를 표시합니다.
- **`OnDisconnected`**: 연결 해제 시 네트워크를 종료하고 에러 이유를 다이얼로그로 출력합니다.
- **`LeaveRpc`**: 서버에서 클라이언트의 플레이어 객체를 제거합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 객체 및 타입들입니다.*
- **RoomStatus (Class)**: 방 상태 정보.
- **SoundManager (Class)**: 사운드 제어 서비스.
- **EndStageMessage, LeaveRoomMessage 등 (Struct/Class)**: 씬 내 통신 메시지.
