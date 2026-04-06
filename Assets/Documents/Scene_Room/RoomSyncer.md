# RoomSyncer.cs 기술 문서

**개요**
`RoomSyncer`는 로비 씬에서 방의 코드, 게임 모드 및 스테이지 선택 정보를 네트워크상에서 동기화하는 `NetworkBehaviour`입니다.

**필드 (Fields)**
- `_sharedCode`: 방 코드를 동기화하는 `NetworkVariable`.
- `_sharedStageInfo`: 현재 선택된 게임 모드 및 스테이지 인덱스를 동기화하는 `NetworkVariable`.
- `_stageList`: 스테이지 목록 데이터를 관리하는 `StageListData`.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 데이터와 메시지 발행기를 초기화하고, 스테이지 모드 및 선택 변경 요청(`ISubscriber`)을 구독합니다.
- **`OnNetworkSpawn` / `OnNetworkDespawn`**: 네트워크 스폰 시 초기 상태를 동기화하고 변경 이벤트 등록/해제를 수행합니다.
- **`SwitchStageMode`**: 서버 환경에서 게임 모드(Coop/Comp)를 전환하고 동기화 변수를 갱신합니다.
- **`SelectStage`**: 서버 환경에서 스테이지 인덱스를 갱신합니다.
- **`OnSelectionChanged`**: 동기화 변수 변경 시 클라이언트 측의 UI 상태를 업데이트하기 위해 메시지(`SwitchModeRespond`, `SelectStageRespond`)를 발행합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 및 타입들입니다.*
- **SelectedStageInfo (Struct)**: 스테이지 정보(모드, 인덱스) 구조체.
- **StageListData (ScriptableObject)**: 스테이지 설정 데이터.
- **다양한 메시지 클래스 (Struct/Class)**: 통신용 메시지.
