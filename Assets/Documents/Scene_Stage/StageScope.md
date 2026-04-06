# StageScope.cs 기술 문서

**개요**
`StageScope`는 스테이지 씬의 의존성 관리 범위를 정의하는 `LifetimeScope`입니다. 게임 로직, UI 프레젠터, 스테이지 데이터 및 네트워크 서비스들을 컨테이너에 등록하고 관리합니다.

**필드 (Fields)**
- `providerData`: 아이템 제공자 관련 설정 데이터(`ProviderData`).
- `sfxListData`: 효과음 목록 데이터(`StageSfxListData`).
- `playerPrefab`: 플레이어 캐릭터 동기화 프리팹(`PlayerSyncer`).

**주요 메서드 (Methods)**
- **`Awake`**: 프레임레이트를 60으로 설정합니다.
- **`Configure(IContainerBuilder builder)`**: DI 컨테이너를 설정합니다.
    - **`RegisterEntryPoint<T>`**: `StageInitiator`, `StageHub`를 엔트리 포인트로 등록합니다.
    - **`Register<T>`**: 버튼 액션(`Settings`, `Leave`), 상태 관리 서비스(`StageStatus`), 브로커(`PlacementBroker`, `ContactBroker`) 등을 등록합니다.
    - **`RegisterInstance`**: 스테이지 데이터, 효과음 리스트, 제공자 데이터 및 플레이어 프리팹을 싱글톤으로 제공합니다.
    - **`RegisterMessageBroker<T>`**: 게임 로직, UI, 네트워크 통신을 위한 다양한 메시지 파이프라인 브로커를 설정합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 및 타입들입니다.*
- **StageSfxListData, ProviderData (ScriptableObject)**: 게임 내 사운드 및 아이템 제공 설정 데이터.
- **PlayerSyncer (Class)**: 플레이어 프리팹.
- **다양한 메시지, 인터페이스 및 서비스 클래스들**: 씬 내 통신 및 게임 로직 정의.
