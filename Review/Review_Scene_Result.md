# Review_Scene_Result.md

## 1. 현황 (Observation)
- `ResultScope.cs`: `VContainer`를 사용하여 씬 전용 서비스(`ResultMemberCardProvider`)와 의존성을 주입하고 있습니다.
- `ResultMemberSyncer.cs`: `RoomMemberSyncer`와 매우 유사한 패턴으로, 네트워크 로드 완료 후 씬에 결과 멤버들을 스폰하고 위치를 재계산(Refresh)합니다.

## 2. 리스크 (Risk)
- **코드 중복:** `ResultMemberSyncer`의 로직(`PrefabHandler` 관리, 멤버 스폰, 이벤트 처리)이 `RoomMemberSyncer`와 거의 90% 이상 동일합니다. 이는 명백한 코드 중복으로, 향후 멤버 관리 로직 변경 시 두 곳을 모두 수정해야 하는 유지보수 리스크가 큽니다.
- **테스트 취약성:** `RoomMemberSyncer`와 마찬가지로 `NetworkBehaviour` 의존성으로 인해 단위 테스트가 어렵습니다.
- **오브젝트 관리:** `ResultMemberSyncer`는 멤버 스폰 시 `SpawnAsPlayerObject`를 사용하는데, 결과 화면에서도 동일한 플레이어 객체 관리 방식을 사용하는 것이 적절한지 검토가 필요합니다(단순한 보여주기용 객체라면 더 가벼운 처리가 가능할 수 있습니다).

## 3. 제안 (Proposal)
- **로직 추상화 (Template/Base Class):** `RoomMemberSyncer`와 `ResultMemberSyncer`의 공통 로직을 `BaseMemberSyncer<T>`와 같은 추상 클래스나 상속 기반의 베이스 클래스로 통합하여 코드 중복을 제거하십시오.
- **객체 관리 최적화:** 결과 화면의 멤버들이 네트워크 상태 동기화가 반드시 필요한지 검토하고, 단순 뷰 데이터라면 네트워크 객체 스폰이 아닌 로컬 스폰으로 전환하여 네트워크 트래픽을 절감하십시오.
- **테스트 코드 도입:** 멤버 위치 계산 로직(`CalculatePosAndRot`)은 네트워크 의존성이 없는 순수 수학 로직이므로, 이를 별도의 유틸리티 클래스로 분리하여 유닛 테스트를 작성하십시오.

## 4. 피드백 (Feedback)
- **점검 결과 (2024-05-24):** 
    - `ResultScope.cs`는 `VContainer`를 통해 씬 필수 서비스와 메시지 브로커를 올바르게 주입하고 있습니다.
    - `ResultMemberSyncer.cs`는 `NetworkBehaviour` 의존성이 높고 `RoomMemberSyncer.cs`와 로직 중복이 존재합니다. 
    - 향후 `BaseMemberSyncer` 추상화 및 네트워크/로컬 스폰 방식 분리가 필요합니다.
