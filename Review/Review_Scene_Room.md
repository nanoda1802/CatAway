# Review_Scene_Room.md

## 1. 현황 (Observation)
- `RoomScope.cs`: `VContainer`를 사용하여 씬 전용 서비스(`PointSwapper`, `RoomMemberCardProvider`)와 의존성을 주입하고 있습니다.
- `RoomSyncer.cs`: `NetworkBehaviour`를 상속받아 방 코드 및 스테이지 선택 상태(`NetworkVariable`)를 동기화하고, `MessagePipe`를 통해 메시지 이벤트를 처리합니다.
- `RoomMemberSyncer.cs`: 멤버의 스폰/디스폰 관리, `NetworkManager`의 `PrefabHandler`를 직접 사용하여 멤버 프리팹을 커스텀하게 관리합니다.

## 2. 리스크 (Risk)
- **로직 혼재:** `RoomMemberSyncer.cs` 내에 `Spawn`, `NetworkEvent`, `Member Management`, `Swap Logic`이 모두 섞여 있어 단일 책임 원칙(SRP)을 위반하고 있습니다.
- **네트워크 위험 요소:** 
    - `RoomMemberSyncer`에서 `OnRoomLoadComplete` 내에서 `NetworkManager.LocalClientId`를 사용하는 방식은 `Host` 환경에 따라 예기치 못한 동작을 할 위험이 있습니다.
    - `OnNetworkDespawn`에서 `UnregisterPrefabHandler`를 수행하지만, `AddHandler` 호출 시점과 씬 로드 타이밍 문제로 등록되지 않은 상태에서 제거하려 할 경우 에러가 발생할 가능성이 있습니다.
- **성능/유지보수:** `RoomMemberSyncer` 내의 `points`(멤버 스폰 포인트)를 수동으로 관리하고 이를 위해 `SwapMember` 콜백을 넘기는 구조가 복잡하여 확장성이 낮습니다.
- **테스트 취약성:** `RoomSyncer`와 `RoomMemberSyncer` 모두 `NetworkBehaviour`를 상속받아 유니티 씬 없이 로직 검증이 불가능합니다.

## 3. 제안 (Proposal)
- **책임 분리 (Refactoring):**
    - `RoomMemberSyncer`에서 멤버 데이터를 관리하는 로직(`RoomStatus` 연동)과 실제 물리적인 스폰 로직(`GameObject` 생성)을 분리하십시오.
    - `Spawn` 로직을 별도의 `IMemberSpawner` 인터페이스 기반 클래스로 추출하여 테스트 가능하게 만드십시오.
- **안전한 핸들러 관리:** `PrefabHandler` 등록은 `OnNetworkSpawn`이 아닌 `NetworkManager` 초기화 시점이나 별도 매니저 클래스에서 중앙 관리하십시오.
- **상태 관리 개선:** `Swap` 로직 등에서 `Debug.Log`에 의존하기보다, 상태 변경 이벤트를 명확히 하여 로직을 추적하십시오.
- **테스트 코드 도입 (Unit Test):** `RoomSyncer`에서 `MessagePipe`를 통해 들어오는 요청 처리(`SwitchStageMode`, `SelectStage`) 로직을 추출하여, 외부 의존성(NetworkVariable 등) 없이 테스트 가능한 단위 로직으로 분리하십시오.

## 4. 피드백 (Feedback)
- **점검 결과 (2024-05-24):** 
    - `RoomScope.cs`는 `VContainer`를 통해 씬 필수 서비스와 메시지 브로커를 올바르게 주입하고 있습니다.
    - `RoomMemberSyncer.cs`는 책임이 과도하게 집중되어 있으며(SRP 위반), 네트워크 이벤트 핸들링 시점의 안전성 보완이 필요합니다. 
    - 향후 `Spawn` 로직의 분리와 중앙 집중식 `PrefabHandler` 관리로 리팩토링이 권장됩니다.
