# Review_Scene_Home.md

## 1. 현황 (Observation)
- `HomeScope.cs`: `VContainer`를 사용하여 씬 전용 서비스 및 UI 액션들을 등록합니다.
- `RoomConnector.cs`: `NetworkBehaviour`를 상속받아 호스트 생성(CreateRoom) 및 클라이언트 참여(JoinRoom) 로직을 처리하며 `MessagePipe`를 통해 UI와 통신합니다.
- `HomeAvatar.cs`: `AvatarData`와 `PlayerStatus`를 사용하여 홈 화면에서 아바타를 표시하고, `AvatarMessage` 구독을 통해 상태를 업데이트합니다.

## 2. 리스크 (Risk)
- **아키텍처/유지보수:** 
    - `RoomConnector`가 `NetworkManager`와 `UnityTransport`에 직접 의존하며, 릴레이/연결 로직이 `UniTask.Delay` 등을 이용한 임시 구현체에 의존하고 있어 확장이 어렵습니다.
    - `HomeAvatar`가 `MonoBehaviour`의 `Construct` 메서드를 통해 DI를 수행하는데, `MessagePipe` 구독이 씬 종료 시 안전하게 해제되는지(`DisposableBag` 등을 사용하지 않고 `_subs`를 사용하는 구조) 확인이 필요합니다.
- **네트워크 위험 요소:** 
    - `RoomConnector`의 `CreateRoom`/`JoinRoom` 비동기 로직 내에서 연결 성공 여부(`networkStarted` 플래그)를 신뢰할 수 없는 문제가 코드 내 주석으로 남겨져 있습니다.
    - 호스트 연결 로직과 UI 상태(`PopUp`) 동기화가 이벤트 기반으로 강하게 결합되어 있어, 연결 실패 시나리오 복구가 복잡합니다.
- **코드 일관성:** `HomeAvatar`는 `[Inject]`를 통한 DI를 사용하지만, 다른 스크립트들과 비교했을 때 `VContainer` 사용 패턴이 파편화되어 있습니다.

## 3. 제안 (Proposal)
- **연결 로직 추상화:** 네트워크 연결 로직을 `IRoomService` 인터페이스로 분리하여 `RoomConnector`로부터 비즈니스 로직을 추출하십시오. 
- **비동기 제어 개선:** `RoomConnector`의 비동기 작업(`UniTask`) 처리에 `CancellationTokenSource`를 명시적으로 도입하여 씬 전환 시 작업이 즉시 취소되도록 보장하십시오.
- **DI 패턴 통일:** `HomeAvatar`와 같은 뷰 컴포넌트의 DI 주입 방식(`Construct`)을 프로젝트의 다른 컴포넌트들과 통일하십시오.
- **에러 핸들링:** `CreateRoom`/`JoinRoom` 실패 시나리오를 명확한 상태 머신으로 관리하여 사용자에게 일관된 피드백을 제공하십시오.
