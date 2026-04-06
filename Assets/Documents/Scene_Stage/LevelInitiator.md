# LevelInitiator.cs 기술 문서

**개요**
`LevelInitiator`는 스테이지 씬 로드 직후, 네트워크 상의 플레이어와 테이블 등 핵심 객체들을 스폰하고 초기화하는 서비스입니다.

**필드 (Fields)**
- `_tablePrefabs`: 씬 내 배치 가능한 테이블 프리팹들의 해시 ID와 객체 딕셔너리.
- `_resolver`: 객체 의존성 주입을 위한 `IObjectResolver`.

**주요 메서드 (Methods)**
- **`Initialize`**: 테이블 프리팹을 캐싱하고, `PlayerPrefabHandler`를 네트워크 시스템에 등록합니다. 서버인 경우 씬 로드 완료 이벤트(`OnLevelLoaded`)를 구독합니다.
- **`Dispose`**: 프리팹 핸들러들을 시스템에서 제거합니다.
- **`OnLevelLoaded`**: 모든 클라이언트의 씬 로드 완료 시 서버 권한으로 플레이어들을 스폰합니다.
- **`CacheTablePrefabs`**: 네트워크 설정에서 태그가 "Table"인 객체들을 찾아 등록하고 `TablePrefabHandler`를 할당합니다.
- **`SpawnPlayers`**: 방 정보(`RoomStatus`)에 기반하여 각 클라이언트별로 플레이어를 스폰합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 및 타입들입니다.*
- **RoomStatus, MemberInfo (Class)**: 룸 상태 및 멤버 정보.
- **PlayerSyncer (Class)**: 플레이어 동기화 컴포넌트.
- **PlayerPrefabHandler, TablePrefabHandler (Class)**: `INetworkPrefabInstanceHandler`를 구현하여 객체의 생성/파괴를 처리하며, 본 클래스에서 사용/관리됩니다.
