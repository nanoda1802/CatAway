# Review_Shared_Data_Avatar_Player_Room.md

## 1. 현황 (Observation)
- `AvatarData.cs`: `ScriptableObject`를 사용하여 아바타 관련 리소스를 관리하며, `MaterialPropertyBlock`을 이용한 최적화가 구현되어 있습니다.
- `PlayerStatus.cs`: `MessagePipe`를 통해 사용자 입력(이름 변경, 아바타 변경)을 동적으로 구독하고 업데이트합니다.
- `RoomStatus.cs`: `Unity.Netcode` 환경에서 방 상태(멤버, 스테이지 등)를 관리하며, `Array` 기반의 고정 크기(4명) 관리를 수행합니다.

## 2. 리스크 (Risk)
- **메모리/성능:** 
    - `RoomStatus`에서 잦은 `Debug.Log` 호출(특히 `Report` 메서드)이 런타임 성능에 영향을 줄 수 있습니다.
    - `AvatarData`에서 배열 인덱스 접근 시 `idx < 0` 등 유효성 검사를 수행하지만, 배열 크기가 가변적이 될 경우 대응이 어렵습니다.
- **아키텍처/유지보수:**
    - `RoomStatus`가 `Unity.Netcode`와 직접 결합되어 있어, 순수 C# 로직 테스트가 어렵습니다.
    - `SetTeamByIndex`와 같이 팀 할당 로직이 하드코딩된 상수(`4`, `2`)를 기반으로 동작하여 유연성이 낮습니다.
- **테스트 취약성:** `RoomStatus`는 `NetworkManager`에 강하게 의존하여 단위 테스트가 거의 불가능한 구조입니다.

## 3. 제안 (Proposal)
- **로직 분리 (Clean Architecture):** `RoomStatus`의 비즈니스 로직(멤버 배치, 팀 결정 등)을 `Unity.Netcode` 의존성이 없는 순수 C# 도메인 클래스로 분리하십시오. 이는 단위 테스트 도입의 필수 전제 조건입니다.
- **성능 최적화:** 
    - 런타임에 불필요한 `Debug.Log`는 제거하거나, 컴파일 조건(`[Conditional("DEBUG")]`)을 활용하십시오.
    - `RoomStatus`의 `Report` 메서드는 디버깅 용도로만 남겨두고 실무 코드에서는 배제하십시오.
- **데이터 구조 개선:** `RoomStatus` 내의 고정 배열 접근 방식을 열거형이나 전략 패턴으로 추상화하여, 추후 방 인원 수가 변경되더라도 로직 수정이 최소화되도록 리팩토링하십시오.
- **테스트 코드 도입 (Unit Test):** `RoomStatus`의 멤버 삽입/삭제/스왑 로직을 도메인 모델로 분리 후, 이를 검증하는 테스트 코드를 작성하십시오. (예: `RoomMemberPlacementTests`)
