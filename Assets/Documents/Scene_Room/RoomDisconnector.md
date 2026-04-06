# RoomDisconnector.cs 기술 문서

**개요**
`RoomDisconnector`는 로비 씬에서 방을 나가거나 연결이 끊겼을 때의 정리 작업 및 씬 전환을 관리하는 `NetworkBehaviour`입니다.

**필드 (Fields)**
- `_roomStatus`: 방의 상태(`RoomStatus`) 관리 객체.
- `_loadScenePub` 등: UI 알림 및 씬 전환을 위한 메시지 발행기들.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 메시지 발행기 및 방 나가기 요청(`LeaveRoomMessage`) 구독을 설정합니다.
- **`OnNetworkSpawn` / `OnNetworkDespawn`**: 네트워크 연결 이벤트(`OnConnection`)를 구독/해제합니다.
- **`LeaveRoom`**: 방 나가기 요청 처리. 서버인 경우 모든 클라이언트를 강제 연결 종료하고, 클라이언트인 경우 `LeaveRpc`를 호출합니다. 이후 `LoadHomeAfterShutdown`을 통해 홈 씬으로 전환합니다.
- **`OnConnection`**: 연결 해제 시 `OnDisconnected`를 호출하여 알림을 표시하고 네트워크를 종료합니다.
- **`LeaveRpc` / `NotifyDisconnectRpc`**: 클라이언트가 방을 나갈 때 서버에서 객체를 처리하고 나머지 클라이언트에게 알림 메시지를 보냅니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 객체 및 타입들입니다.*
- **RoomStatus (Class)**: 방 상태 정보.
- **LeaveRoomMessage, RoomToastMessage 등 (Struct/Class)**: 씬 내 통신 메시지.
