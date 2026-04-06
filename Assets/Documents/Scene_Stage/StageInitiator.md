# StageInitiator.cs 기술 문서

**개요**
`StageInitiator`는 스테이지 씬 내의 씬 로딩 흐름을 제어하고, 게임 스테이지의 시작/종료에 따른 사운드 처리를 담당하는 서비스입니다.

**필드 (Fields)**
- `_netManager`: 네트워크 관리자.
- `_sceneChanger`: 서버 환경에서 추가 씬 로드를 관리하는 서비스.
- `_room`: 방 상태(`RoomStatus`) 관리 객체.
- `_soundManager`: BGM 및 SFX 제어를 위한 사운드 관리자.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 서비스들을 초기화하고, 스테이지 시작(`StartStageMessage`) 및 종료(`EndStageMessage`) 메시지를 구독합니다.
- **`Initialize` / `Dispose`**: 네트워크 씬 로드 이벤트 구독 및 해제를 수행합니다.
- **`OnAllClientsCompleted`**: 서버 권한에서 씬 로드가 완료될 때마다, 현재 단계에 따라 스테이지 UI 또는 레벨 씬을 추가 로드(`Additive`)합니다.
- **`StartBgm` / `StopBgm`**: 게임 스테이지의 시작과 종료에 맞춰 배경음악(BGM)을 재생하거나 정지합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 객체 및 타입들입니다.*
- **SceneChanger (Class)**: 씬 전환 관리 서비스.
- **RoomStatus (Class)**: 방 상태 정보.
- **SoundManager (Class)**: 사운드 제어 서비스.
- **StartStageMessage, EndStageMessage (Struct/Class)**: 스테이지 생명주기 통신 메시지.
