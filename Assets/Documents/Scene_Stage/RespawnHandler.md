# RespawnHandler.cs 기술 문서

**개요**
`RespawnHandler`는 스테이지에서 플레이어가 디스폰되었을 때, 일정 시간 후 자동으로 리스폰시키고 리스폰 대기 UI(`RespawnCard`)를 관리하는 서버 측 네트워크 서비스입니다.

**필드 (Fields)**
- `_waiters`: 리스폰 대기 중인 플레이어들을 관리하는 딕셔너리(`RespawnWaiter`).
- `_activeCards`, `_inactiveCards`: 리스폰 카드 객체의 풀링(Pool) 시스템.
- `_respawnQueue`: 리스폰 처리가 필요한 플레이어 대기열.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 플레이어 디스폰 메시지(`PlayerDespawnMessage`)를 구독하고 리스폰 카드 풀을 초기화합니다.
- **`NetworkUpdate`**: 매 프레임 리스폰 대기 타이머를 갱신하고, 종료된 플레이어를 리스폰 큐에 등록하여 처리합니다.
- **`AddWaiter`**: 플레이어 디스폰 메시지 수신 시 리스폰 대기열에 추가하고 `NetworkUpdate`를 시작하며, 전체 클라이언트에 카드 활성화 RPC를 호출합니다.
- **`RemoveWaiter`**: 리스폰 대기 시간 종료 시 호출되며, UI 카드를 숨기고 플레이어 객체를 재생성(`RespawnPlayer`)합니다.
- **`RespawnPlayer`**: 플레이어 객체를 프리팹으로부터 재생성하고 네트워크상에 스폰합니다.
- **`ShowCardRpc` / `HideCardRpc`**: 클라이언트들에 리스폰 대기 카드를 표시하거나 숨기는 RPC입니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 및 타입들입니다.*
- **RespawnWaiter (Class)**: 개별 플레이어의 리스폰 타이머 관리.
- **RespawnCard, RespawnCardData (Class/ScriptableObject)**: 리스폰 카드 UI 및 설정 데이터.
- **PlayerSyncer (Class)**: 플레이어 프리팹.
- **PlayerDespawnMessage (Struct/Class)**: 플레이어 디스폰 통신 메시지.
