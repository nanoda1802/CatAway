# SessionManager.cs 기술 문서

**개요**
`SessionManager`는 네트워크 세션의 접속 승인(Connection Approval) 로직을 관리하는 서비스입니다. 서버 시작 시 연결 요청을 검증하고 승인 여부를 결정합니다.

**필드 (Fields)**
- `_netManager`: `Unity.Netcode.NetworkManager` 인스턴스.
- `_roomStatus`: 현재 방의 상태(`RoomStatus`) 정보.

**주요 메서드 (Methods)**
- **`Initialize` / `Dispose`**: 네트워크 상태 이벤트(`OnServerStarted`, `OnPreShutdown`)를 구독/해제합니다.
- **`OnHostStarted` / `OnPreShutdown`**: 서버 시작/종료 시 접속 승인 콜백(`ApprovalCheck`)을 설정 및 해제합니다.
- **`ApprovalCheck`**: 클라이언트 연결 요청 시 호출되며, 다음 기준에 따라 승인 여부를 결정합니다.
    - 서버 최대 접속자 수(4명) 초과 여부.
    - 대상 방(`_roomStatus`)의 만석 여부.
    - 클라이언트 ID 중복 여부.
    - (추가 예정) 스테이지 진행 중 여부.
    - 승인 시 `CreatePlayerObject`를 false로 설정하여 자동 플레이어 생성을 방지합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 객체입니다.*
- **RoomStatus (Class)**: 현재 방의 상태를 관리하는 데이터 클래스입니다.
