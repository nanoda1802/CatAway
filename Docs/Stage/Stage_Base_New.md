`C:\Users\home\Documents\GitHub\PartTimeCat\Assets\_Scripts\Stage`

# Stage 모듈: 베이스 시스템

이 문서는 `Assets/_Scripts/Stage/` 디렉토리에 위치한 스테이지의 전반적인 라이프사이클 관리, 네트워크 제어, 서비스 허브 등 핵심 인프라 스크립트에 대해 설명합니다.

## 1. 개요 (Overview)
스테이지의 초기화, 플레이어 스폰, 네트워크 접속 해제, 씬 전환 및 스테이지 전역 의존성 주입을 담당하는 스테이지 모듈의 기반 시스템입니다.

## 2. 핵심 라이프사이클 관리

*   **StageScope.cs**: `LifetimeScope`를 상속받아 스테이지 내 모든 서비스와 컴포넌트의 의존성을 설정합니다. 
    *   **주요 의존성**: `StageStatus`(싱글톤), `PlacementBroker`(싱글톤), `ContactBroker`(싱글톤), `PlayerSyncer`(플레이어 프리팹 인스턴스), `DisposableBagBuilder`.
    *   **메시지 브로커 등록**: `IPlacable`, `IProvider`, `OrderPresenter`, `ScorePresenter`, `CuePresenter`, `HubCallMessage` 등 스테이지 전역 메시지 브로커를 `MessagePipe`를 통해 등록합니다.
*   **StageInitiator.cs**: 스테이지의 초기화 로직을 수행합니다. `NetworkManager.SceneManager`의 이벤트를 구독하여 스테이지 씬 로딩 완료 시 UI 씬과 레벨 씬을 `Additive` 방식으로 로드하며, 스테이지 시작/종료 메시지에 맞춰 BGM 재생을 관리합니다.
*   **LevelInitiator.cs**: 레벨 로딩 완료 후 테이블 및 플레이어 생성(`Spawn`)을 담당하며, `NetworkPrefabHandler`를 통해 네트워크 객체 생성을 제어합니다.

## 3. 스테이지 구성 요소 관리 (StageHub)

*   **StageHub.cs**: 스테이지 내 주요 구성 요소(Providers, Placables, Presenters)들이 서로를 직접 참조하지 않고도 필요 시 상호작용할 수 있도록 연결해주는 **접근 거점**입니다.
    *   **역할**: 스테이지 내의 테이블, 아이템 공급자, 프레젠터 등을 메시지 구독을 통해 중앙에서 유지 관리하며, 다른 모듈에서 이들의 참조가 필요할 때 해당 객체를 발급(`Fetch`)해주는 역할을 합니다. 이는 시스템 간 결합도를 낮추는 핵심적인 느슨한 결합 구조입니다.

## 4. 네트워크 및 상태 제어

*   **RespawnHandler.cs**: 플레이어 사망 시 부활을 관리합니다. `PlayerDespawnMessage`를 구독하여 서버에서 부활 쿨타임을 관리하고, 부활 카운트다운을 위해 `RespawnCard`를 제어합니다.
*   **StageDisconnector.cs**: 네트워크 연결 종료, 방 나가기 등의 요청을 처리합니다. 클라이언트 접속 해제 시 적절한 씬으로의 복귀나 종료 처리를 수행합니다.

## 5. 인터페이스 및 기타
*   **DespawnZone.cs**: 특정 영역(예: 맵 밖)에 진입한 아이템이나 플레이어를 자동으로 데스폰시키는 관리 컴포넌트입니다. `IDespawnable` 인터페이스를 구현한 객체들을 관리합니다.
*   **IProvider / IDespawnable**: 객체 공급자 및 데스폰 로직을 위한 표준 인터페이스입니다.

---
## Feedback

- 2024-05-22: 문서 점검 완료. 코드 구조와 문서가 일치함. StageScope, StageInitiator, LevelInitiator, StageHub, RespawnHandler 등 스테이지 기반 인프라 로직이 정확하게 기술되어 있음. 추가 수정 사항 없음.
