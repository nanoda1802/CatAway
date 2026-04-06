# RoomMemberSyncer.cs 기술 문서

**개요**
`RoomMemberSyncer`는 로비 씬에서 네트워크상의 모든 `RoomMember` 객체들을 스폰하고, 위치를 관리하며 상태를 동기화하는 `NetworkBehaviour`입니다.

**필드 (Fields)**
- `points`: 씬에 배치된 멤버 스폰 위치(`MemberPoint`) 배열.
- `_members`: 클라이언트 ID별로 관리되는 멤버 객체 딕셔너리.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 서비스들을 초기화하고, 게임 시작 조건 및 모드 전환 메시지를 구독합니다.
- **`OnNetworkSpawn` / `OnNetworkDespawn`**: 네트워크 스폰/디스폰 시 로드 이벤트 등록, 핸들러 등록(`RoomMemberPrefabHandler`), 연결 이벤트 감지를 수행합니다.
- **`AddMember` / `RemoveTargetMember`**: 서버 환경에서 클라이언트 연결/해제 시 멤버를 스폰하거나 해제합니다.
- **`SwapMember`**: 서버 환경에서 멤버 간 위치 교체를 처리하고 레디 상태를 초기화합니다.
- **`CheckStartState`**: 모든 멤버의 레디 상태를 확인하여 스테이지 시작 메시지를 발행합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 및 타입들입니다.*
- **MemberInfo, RoomStatus (Class)**: 룸 멤버 정보 및 방 상태 관리.
- **RoomMemberPrefabHandler (Class)**: `INetworkPrefabInstanceHandler`를 구현하여 `RoomMember`의 생성과 파괴를 담당합니다. 본 클래스에서 관리됩니다.
- **다양한 메시지 클래스 (Struct/Class)**: 통신용 메시지.
