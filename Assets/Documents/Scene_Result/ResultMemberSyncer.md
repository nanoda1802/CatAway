# ResultMemberSyncer.cs 기술 문서

**개요**
`ResultMemberSyncer`는 결과 씬에서 네트워크상의 모든 `ResultMember` 객체들을 관리하고 동기화하는 `NetworkBehaviour`입니다.

**필드 (Fields)**
- `posOffsetX`, `rotOffsetY`, `posZ`: 멤버들의 위치 및 회전 정렬을 위한 설정값들.
- `_members`: `Dictionary`를 통해 클라이언트 ID별로 관리되는 멤버 객체 리스트.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 멤버 프리팹과 서비스들을 초기화합니다.
- **`OnNetworkSpawn` / `OnNetworkDespawn`**: 씬 로드 완료 이벤트 등록, 네트워크 연결/해제 이벤트 감지 및 `ResultMemberPrefabHandler`의 등록/해제를 수행합니다.
- **`SpawnMembers` / `RefreshMembers`**: 결과 씬 입장 시 멤버들을 생성하거나, 클라이언트 이탈 시 위치를 재정렬합니다.
- **`CreateMemberObject`**: `VContainer`의 `IObjectResolver`를 사용하여 멤버 객체를 인스턴스화합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 및 타입들입니다.*
- **MemberInfo, RoomStatus (Class)**: 룸 멤버 정보 및 방 상태 관리.
- **ResultMemberPrefabHandler (Class)**: `INetworkPrefabInstanceHandler`를 구현하여 `ResultMember`의 생성과 파괴를 담당합니다. 이 핸들러는 본 `ResultMemberSyncer`에 의해 생성 및 관리됩니다.
