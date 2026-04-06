# ResultInitiator.cs 기술 문서

**개요**
`ResultInitiator`는 결과(Result) 씬이 로드된 후 게임 결과를 초기화하고 처리하도록 신호를 발행하는 서비스입니다. 서버에서 모든 클라이언트의 씬 로드 완료를 감지하면 시작 메시지를 발행합니다.

**필드 (Fields)**
- `_netManager`: `Unity.Netcode.NetworkManager` 인스턴스.
- `_startPub`: 게임 결과 처리를 시작하기 위한 메시지 발행기.

**주요 메서드 (Methods)**
- **`Initialize`**: 씬 로드 완료 이벤트(`OnLoadEventCompleted`)를 구독합니다.
- **`OnAllClientsCompleted`**: 모든 클라이언트의 씬 로드가 완료되면, 서버 환경에서 `StartResultMessage`를 발행하여 결과 화면 표시를 트리거합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 메시지 객체입니다.*
- **StartResultMessage (Struct/Class)**: 결과 씬 시작 메시지.
