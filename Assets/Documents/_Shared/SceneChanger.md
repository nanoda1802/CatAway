# SceneChanger.cs 기술 문서

**개요**
`SceneChanger`는 애플리케이션의 씬 전환을 관리하는 서비스입니다. 로컬 씬 로드와 네트워크(Netcode for GameObjects) 기반의 서버 동기화 씬 로드를 모두 지원합니다.

**필드 (Fields)**
- `_netManager`: `Unity.Netcode.NetworkManager` 인스턴스.
- `InNetwork`: 네트워크 연결 상태 여부를 확인하는 속성.

**주요 메서드 (Methods)**
- **`Initialize` / `Dispose`**: `IInitializable` 및 `IDisposable` 인터페이스를 구현하여 네트워크 매니저의 이벤트 구독/해제를 관리합니다.
- **`SubscribeSceneEvents` / `UnsubscribeSceneEvents`**: `NetworkSceneManager`의 씬 로드 관련 이벤트(`OnLoad`, `OnLoadComplete`, `OnLoadEventCompleted`)를 관리합니다.
- **`HandleMessage(LoadSceneMessage msg)`**: `LoadSceneMessage`를 수신하여, 네트워크 상태에 따라 로컬 로드(`LoadSelf`) 또는 서버 로드(`LoadByServer`)를 호출합니다.
- **`LoadByServer`**: 서버 권한이 있는 경우 `NetworkSceneManager`를 통해 모든 클라이언트의 씬 로드를 시작합니다.
- **`OnLoadStarted` / `OnLocalCompleted` / `OnAllClientsCompleted`**: 씬 로드 과정의 이벤트를 로그로 기록하여 상태를 추적합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 객체입니다.*
- **LoadSceneMessage (Struct/Class)**: 씬 전환 요청을 위한 메시지 데이터입니다.
