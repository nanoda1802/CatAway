`C:\Users\home\Documents\GitHub\PartTimeCat\Assets\_Scripts\Stage\UI`

# Stage UI System

본 문서는 `Stage` 씬에서 게임플레이 정보를 전달하고 사용자 상호작용을 처리하는 UI 시스템 전반에 대해 설명합니다. 본 시스템은 크게 전역 정보를 표시하는 **Board**, 월드 공간의 동적 정보를 표시하는 **Widget**, 게임 흐름 알림을 담당하는 **Pop**으로 구성됩니다.

## 1. 개요 (Overview)

Stage UI 시스템은 서버(Server)와 클라이언트(Client) 간의 긴밀한 네트워크 동기화를 기반으로 작동합니다. 주요 데이터 처리는 서버 측 `Presenter`들이 담당하며, `MessagePipe`를 통해 발행된 메시지를 각 UI 컴포넌트가 구독하여 시각적으로 표현합니다. 효율적인 리소스 관리를 위해 위젯 시스템은 오브젝트 풀링(`ObjectPool`)을 사용합니다.

## 2. Scope & Infrastructure

UI 시스템은 VContainer의 `LifetimeScope`를 통해 의존성을 관리하며, 역할에 따라 두 개의 주요 스코프로 나뉩니다.

### BoardUiScope
- **역할**: 스테이지 전체 상황(점수, 시간, 주문)을 표시하는 메인 보드 UI들의 의존성을 관리합니다.
- **주요 등록 요소**:
    - `BoardUiData`: 보드 UI의 시각적 설정(색상, 연출 등) 데이터.
    - `OrderCardData`, `RespawnCardData`: 개별 카드들의 프리팹 및 설정 데이터.
    - `DisposableBagBuilder`: 구독 해제를 관리하기 위한 빌더.

### WidgetUiScope
- **역할**: 게임 세트 내에서 동적으로 생성/소멸되는 위젯(토스트, 진행 바 등)의 의존성을 관리합니다.
- **주요 등록 요소**:
    - `WidgetData`: 위젯의 위치 오프셋, 색상, 애니메이션 설정 데이터.
    - `Canvas`, `Camera`: 위젯이 렌더링될 전용 캔버스와 메인 카메라.

---

## 3. 핵심 로직 컴포넌트 (Presenters)

서버 측에서 로직을 계산하고 클라이언트에 데이터를 전파하는 핵심 컴포넌트들입니다.

- **`ScorePresenter`**: 
    - 팀별 점수와 콤보를 관리합니다.
    - 요리 완성 시 남은 시간 배율과 콤보 배율을 적용하여 최종 점수를 산출합니다.
    - `UpdateRpc`를 통해 `ScoreMessage`를 모든 클라이언트에 발행합니다.
- **`TimerPresenter`**: 
    - `NetworkUpdateSystem`을 사용하여 서버 시간을 기준으로 스테이지 잔여 시간을 계산합니다.
    - 시간이 종료되면 `CuePresenter`를 통해 종료 연출을 시작하고 `EndStageMessage`를 발행합니다.
- **`OrderPresenter`**: 
    - 주기적으로 새로운 주문을 생성하고 만료된 주문을 처리합니다.
    - 플레이어가 제출한 요리(`Recipe`)가 활성 주문과 일치하는지 확인하고 점수 업데이트를 요청합니다.
- **`CuePresenter`**: 
    - 스테이지의 시작(`Ready? -> Let's Work!`)과 종료(`Timeout!`) 연출의 흐름을 제어합니다.
    - `UniTask`를 사용하여 연출 타이밍을 관리하며, 연출 완료 후 스테이지 시작 또는 결과 씬 전환을 수행합니다.

---

## 4. UI 요소 (UI Elements)

### Boards (상태 정보 판)
- **`ScoreBoard`**: `ScoreMessage`를 구독하여 팀별 점수와 콤보를 표시합니다. `PrimeTween`을 사용해 점수 카운팅 연출을 수행하며, `TeamMessageFilter`로 팀별 메시지를 구분합니다.
- **`TimerBoard`**: 남은 시간을 Fill Bar와 텍스트(M:S)로 표시합니다. 피버 타임(Fever Time) 진입 시 경고 연출과 사운드를 재생합니다.
- **`OrderBoard`**: 팀별 주문 카드들을 관리하는 컨테이너입니다. 새로운 주문이 들어오면 `OrderCard`를 생성하여 정렬하며, 주문 만료 시 카드를 제거하고 재정렬합니다.
- **`OrderCard`**: 개별 주문의 재료 아이콘과 남은 시간 게이지를 표시합니다. 만료 시간이 임박하면 흔들림 연출(`Warn`)을 수행합니다.

### Widgets (동적 위젯)
- **`WidgetProvider<T>`**: 위젯의 생성, 소멸 및 오브젝트 풀링을 담당하는 베이스 클래스입니다. `IProvider` 인터페이스를 통해 시스템에 등록됩니다.
- **`ToastWidget`**: 점수 획득/차감 시 월드 좌표에서 생성되어 솟아오르는 텍스트입니다. 연출 완료 후 자동으로 풀에 반환됩니다.
- **`TableAlertWidget`**: 빈 접시 필요 등 주의가 필요한 테이블 위에 표시되는 깜빡이는 아이콘입니다.
- **`ProgressBarWidget`**: 상호작용(요리 등)의 진행도를 시각화합니다.
- **`PlatingIconWidget`**: 접시 위에 놓인 재료들의 아이콘을 표시하며, 타겟 오브젝트를 실시간으로 추적합니다.

### Pops & Others
- **`CuePop`**: 화면 중앙에 거대한 텍스트로 게임의 시작과 종료를 알리는 팝업입니다.
- **`RespawnCard`**: 플레이어 사망 시 부활 대기 시간을 월드 UI로 표시합니다.
- **`Virtual Input`**: 모바일 플랫폼에서만 활성화되는 가상 조이스틱 및 버튼 시스템입니다.

---

## 5. 주요 데이터 및 메시지 정의

- **`ScoreMessage`**: 팀, 현재 점수, 콤보, 득점 여부를 포함하는 네트워크 동기화 구조체입니다.
- **`AddOrderMessage` / `RemoveOrderMessage`**: 주문의 추가와 제거(성공/실패 여부 포함) 정보를 전달합니다.
- **`CueMessage`**: 표시할 큐의 유형(Start/End)과 지속 시간 정보를 담고 있습니다.
- **`ITeamMessage`**: 팀 구분이 필요한 메시지들이 구현하는 인터페이스로, `TeamMessageFilter`를 통해 특정 팀의 UI만 갱신하는 데 사용됩니다.

---
*참고: 본 문서는 `Assets/_Scripts/Stage/UI/` 하위 스크립트 분석을 바탕으로 작성되었습니다.*

## Feedback
- 점검 결과: `Assets/_Scripts/Stage/UI` 하위 스크립트 구조와 문서 내용이 일치함을 확인했습니다. UI 시스템의 구성 요소(Board, Widget, Pop)와 네트워크 기반 데이터 처리 구조(`MessagePipe` 활용, 서버측 `Presenter`), 그리고 VContainer 스코프 관리 방식(`BoardUiScope`, `WidgetUiScope`)이 정확히 기술되어 있습니다. UI 컴포넌트들의 역할과 데이터 동기화 흐름도 코드 기반으로 올바르게 설명되어 있습니다. 추가 수정 사항 없습니다.
