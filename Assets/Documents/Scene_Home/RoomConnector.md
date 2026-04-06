# RoomConnector.cs 기술 문서

**개요**
`RoomConnector`는 로비에서 네트워크 방(Room) 생성 및 참가 기능을 담당합니다. `NetworkBehaviour`를 상속받아 호스트/클라이언트 연결을 관리하며, 연결 실패 시 사용자에게 알림을 제공합니다.

**필드 (Fields)**
- `_netManager`: 네트워크 연결을 담당하는 `NetworkManager` 인스턴스.
- `_utp`: 네트워크 전송을 위한 `UnityTransport`.
- `_roomStatus`: 방 상태(`RoomStatus`) 관리 클래스.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 메시지 발행/구독을 설정하고 연결 이벤트(`OnConnection`)를 구독합니다.
- **`OnNetworkSpawn` / `OnNetworkDespawn`**: 네트워크 객체 생성 시 씬 로드를 시도하고, 제거 시 이벤트를 해제합니다.
- **`OnConnection`**: 클라이언트 연결 이벤트를 감지하여 승인 실패 시 사용자에게 다이얼로그를 표시합니다.
- **`CreateRoom`**: 비동기적으로 방 생성 로직을 처리하고 `NetworkManager.StartHost()`를 호출합니다.
- **`JoinRoom`**: 입력받은 코드로 `UnityTransport` 설정을 변경하고 `NetworkManager.StartClient()`를 호출하여 방에 참가합니다.
- **`SendDialog`**: 연결 실패 등 상황에서 사용자에게 알림 팝업(`PopUpMessage`, `DialogMessage`)을 발행합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 객체 및 타입들입니다.*
- **RoomStatus (Class)**: 방 상태 정보.
- **LoadSceneMessage, PopUpMessage, PopDownMessage, DialogMessage, CreateRoomRequest, JoinRoomRequest (Struct/Class)**: 통신 및 UI 팝업 메시지.
- **DialogButtonType (Enum)**: 다이얼로그 버튼 유형.
