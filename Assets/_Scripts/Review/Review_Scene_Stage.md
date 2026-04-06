# Review_Scene_Stage.md

## 1. 현황 (Observation)
- `StageHub.cs`: `MessagePipe`를 통해 `IPlacable`, `IProvider` 등의 서비스 인터페이스들을 등록/관리하는 중앙 허브 역할을 수행합니다.
- `StageInitiator.cs`: `NetworkManager.SceneManager`를 사용하여 씬 로드 이벤트를 처리하며, 스테이지 전환 시 필요한 서브 씬을 `Additive`하게 로드합니다.
- `RespawnHandler.cs`: `NetworkBehaviour`와 `INetworkUpdateSystem`을 결합하여 플레이어 디스폰/리스폰 대기열을 관리하고, 리스폰 시 프리팹을 동적으로 스폰합니다.
- `DespawnZone.cs`: `Collider` 기반 트리거로 플레이어/아이템을 감지하여 `IDespawnable` 인터페이스를 호출합니다.

## 2. 리스크 (Risk)
- **로직 결합도:** `StageInitiator`가 씬 이름(`"Stage"`, `"Level"`)을 하드코딩하여 참조하고 있어, 씬 구성 변경 시 매우 취약합니다.
- **성능/유지보수:** 
    - `RespawnHandler`에서 매 프레임 `NetworkUpdate`를 통해 대기자를 체크하는 것은 시스템 규모가 커질 경우 부담이 될 수 있습니다.
    - `StageHub`는 등록된 객체들을 `Dictionary<Type, ...>` 형태로 보관하는데, 잘못된 타입 주입 시 런타임 에러 발생 위험이 있습니다.
- **네트워크 위험 요소:** 
    - `RespawnHandler.RespawnPlayer`에서 `Instantiate` 후 `SpawnAsPlayerObject`를 호출하는데, 네트워크 클라이언트 간 상태 동기화 타이밍 문제(Race Condition)가 발생할 수 있습니다.
    - `DespawnZone`은 `transform.parent`를 참조하여 `IDespawnable`을 찾는데, 프리팹 구조가 변경되면 로직이 깨질 위험이 있습니다.

## 3. 제안 (Proposal)
- **씬 관리 추상화:** 씬 로드 로직(`StageInitiator`)을 하드코딩된 문자열 기반에서 `ScriptableObject` 기반의 설정 데이터 또는 상태 머신으로 전환하십시오.
- **안전한 참조 (Type Safety):** `StageHub`에서 타입 기반 등록 시, 인터페이스를 명확히 하고 컴파일 타임에 타입 안전성을 강화하십시오.
- **리스폰 로직 최적화:** 
    - `RespawnHandler`의 리스폰 로직을 `NetworkObject` 풀링 기술과 결합하여 잦은 `Instantiate`/`Destroy`를 방지하십시오.
    - `DespawnZone`이 특정 계층 구조에 의존하지 않도록, `GetComponentInParent<IDespawnable>()` 등을 사용하거나, 컴포넌트 간 명시적인 참조 구조를 설계하십시오.
- **테스트 코드 도입:** `StageHub` 내 서비스 등록 로직과 `RespawnHandler`의 대기열 처리 로직을 도메인 클래스로 분리하여, 네트워크 없는 단위 테스트를 수행하십시오.
