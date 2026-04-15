# Review_Shared_RootScope_SessionManager.md

## 1. 현황 (Observation)
- `RootScope.cs`에서 `VContainer`를 사용하여 여러 서비스와 컴포넌트를 등록하고 관리하고 있습니다.
- `SessionManager.cs`는 `NetworkManager`의 연결 승인(ApprovalCheck) 로직을 관리하며 `RoomStatus`에 의존합니다.
- `RootScope`의 `Configure` 내 `UseEntryPoints`와 `RegisterInstance`가 혼재되어 사용되고 있습니다.

## 2. 리스크 (Risk)
- **하드코딩:** `SessionManager.cs`의 `ApprovalCheck` 메서드 내부에서 최대 클라이언트 수(`4`)가 하드코딩되어 있습니다. 이는 확장성 문제를 야기할 수 있습니다.
- **의존성 복잡도:** `RootScope`가 너무 많은 `Data` 객체 및 컴포넌트를 직접 `SerializeField`로 주입받고 있어, 환경 변경 시 유지보수가 어렵습니다.
- **코드 일관성:** `Configure` 메서드 내에서 `UseEntryPoints`와 `RegisterInstance`를 병행하여 의존성을 주입하고 있어, 서비스의 생명주기 관리가 일관되지 않을 수 있습니다.
- **테스트 취약성:** `SessionManager`가 `NetworkManager`와 `RoomStatus`에 강하게 결합되어 있어, 유니티 환경 외부에서 단위 테스트를 수행하기 매우 어렵습니다.

## 3. 제안 (Proposal)
- **하드코딩 제거:** `SessionManager`의 설정값들을 별도의 `ScriptableObject`나 설정 전용 클래스로 분리하여 관리하십시오.
- **의존성 분리:** `RootScope`에서 직접 주입하는 대신, 환경 설정 전용 `Provider`를 설계하여 주입하십시오.
- **테스트 코드 도입 (Unit Test):** `SessionManager`의 `ApprovalCheck` 로직을 분리하여, 순수 C# 로직 단위로 테스트 코드를 작성하십시오. (Unity Test Framework의 `Integration Test` 보다는 `Unit Test`를 먼저 목표로 함)
- **코드 스타일:** `_Scripts` 컨벤션에 맞춰 필드 선언 및 메서드 호출 일관성을 검토하십시오.
