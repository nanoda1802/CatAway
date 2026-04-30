# Review_Scene_Stage_UI.md

## 1. 현황 (Observation)
- `ScorePresenter.cs`: 점수 계산 및 갱신을 담당하며, `StageStatus`를 통해 상태를 기록하고 메시지를 발행합니다.
- `OrderPresenter.cs`: 주문 생성, 만료 타이머 관리, 아이템 검증 및 점수 반영을 총괄하는 핵심 로직 컴포넌트입니다. `INetworkUpdateSystem`을 사용하여 실시간으로 게임 상태를 관리합니다.
- `WidgetProvider<T>.cs`: `ObjectPool`을 활용하여 UI 위젯(ProgressBar, Toast 등)을 효율적으로 스폰/디스폰하는 제네릭 기반의 관리자입니다.

## 2. 리스크 (Risk)
- **로직 밀집 (Fat Component):** `OrderPresenter`에 주문 생성 주기, 타이머 틱, 아이템 검증, 점수 반영 등 너무 많은 책임이 집중되어 있습니다. 시스템이 복잡해질수록 테스트와 디버깅이 매우 어려워질 것입니다.
- **네트워크 동기화:** `AddRpc`, `RemoveRpc` 등을 통해 상태를 모든 클라이언트에 브로드캐스트하는데, 씬 중간 진입 시점이나 네트워크 지연 환경에서 클라이언트가 서버와 일치하지 않는 상태(Desync)에 빠질 위험이 있습니다.
- **성능:** `OrderPresenter`가 `NetworkUpdate`의 `EarlyUpdate`와 `Update`를 동시에 사용하여 매 프레임 수많은 체크(Timer 업데이트, interval 체크)를 수행하고 있어 오버헤드가 발생할 수 있습니다.
- **유지보수:** `WidgetProvider<T>`는 `IProvider` 인터페이스를 구현하고 `StageHub`와 메시지로 통신하는데, 시스템 내 위젯 개수가 늘어날 때 DI 등록 및 메시지 파이프 관리가 복잡해질 수 있습니다.

## 3. 제안 (Proposal)
- **로직 분리:** `OrderPresenter`에서 타이머 로직과 주문 생성/삭제 로직을 별도의 도메인 클래스(예: `OrderManager`)로 분리하여 단위 테스트 가능하게 만드십시오.
- **네트워크 동기화 신뢰성:** 현재 `RPC` 기반 메시지 통신을 `NetworkVariable` 또는 `NetworkList` 기반으로 전환하여 상태 동기화의 안정성을 높이십시오.
- **성능 최적화:** `NetworkUpdate`의 주기를 프레임 단위 체크 대신 `UniTask.Delay` 등을 활용한 코루틴/태스크 주기로 완화하여 오버헤드를 줄이십시오.
- **위젯 시스템 확장:** `WidgetProvider`를 `LifetimeScope`와 결합하여, 각 씬별로 필요한 위젯만 로드하도록 최적화하고 메시지 파이프 의존성을 줄이십시오.
