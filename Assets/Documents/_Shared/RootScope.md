# RootScope.cs 기술 문서

**개요**
`RootScope`는 애플리케이션의 최상위 DI(Dependency Injection) 컨테이너를 정의하며, `VContainer`를 사용하여 애플리케이션 전반의 서비스와 컴포넌트를 등록하고 관리합니다.

**필드 (Fields)**
- **[SF] `netManager`**: Unity `NetworkManager` 인스턴스.
- **[SF] `utp`**: 네트워크 전송 계층인 `UnityTransport` 설정.
- **[SF] `soundManager`**: 게임 내 사운드를 제어하는 `SoundManager` 인스턴스.
- **[SF] `stageList`**: 게임 내 스테이지 목록 데이터(`StageListData`).
- **[SF] `avatarData`**: 플레이어 아바타 관련 설정 데이터(`AvatarData`).
- **[SF] `soundSettingsData`**: 사운드 시스템 설정 데이터(`SoundSettingsData`).
- `_rootDisposableBagBuilder`: 애플리케이션 수명 주기 동안 관리되는 `Disposable` 객체들의 컨테이너.

**주요 메서드 (Methods)**
- **`Configure(IContainerBuilder builder)`**: DI 컨테이너에 서비스와 인스턴스를 등록하는 핵심 설정 메서드입니다.
    - **`UseEntryPoints`**: `SessionManager`, `RoomStatus`, `SceneChanger`, `PlayerStatus`, `SfxProvider` 등 핵심 시스템을 엔트리 포인트로 등록합니다.
    - **`UseComponents`**: `netManager`, `utp`, `soundManager` 등 씬 컴포넌트를 주입합니다.
    - **`RegisterInstance`**: 정적 데이터들(`stageList`, `soundSettingsData`, `avatarData`)을 싱글톤으로 제공합니다.
    - **`Register<T>`**: `PlayerInput`, `VfxHandler`, `TweenHandler` 등을 싱글톤으로 등록합니다.
- **`RegisterRootDisposableBag`**: 전역 `DisposableBagBuilder`를 컨테이너에 등록합니다.
- **`RegisterMessages`**: `MessagePipe`를 초기화하고 `LoadSceneMessage`, `AvatarMessage`, `RenameMessage`를 위한 메시지 브로커를 등록합니다.
- **`OnDestroy`**: 앱 종료 시 `_rootDisposableBagBuilder`를 빌드하여 모든 리소스를 해제합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 ScriptableObject 및 데이터 객체입니다.*
- **StageListData, AvatarData, SoundSettingsData (ScriptableObject)**: 게임 내 설정을 담당하는 데이터 컨테이너입니다.
- **LoadSceneMessage, AvatarMessage, RenameMessage (Struct/Class)**: 전역 메시지 통신을 위한 데이터 객체입니다.
