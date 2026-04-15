# Review_Scene_Stage_Player.md

## 1. 현황 (Observation)
- `PlayerSyncer.cs`: 플레이어의 네트워크 상태(`AvatarIndex`), 입력 활성/비활성, 디스폰 로직을 총괄하는 핵심 컴포넌트입니다. `MessagePipe`를 통해 `Start`/`End` 메시지를 구독하여 제어합니다.
- 구조적으로 `PlayerSyncer`가 `Behaviour`들에 대한 의존성을 `IReadOnlyList<IBehaviourWithInput>` 형태로 주입받아 관리하는 것이 특징입니다.

## 2. 리스크 (Risk)
- **로직 의존성:** `PlayerSyncer` 내부에 주석으로 적힌 내용들처럼 `OnNetworkSpawn`과 `OnNetworkPostSpawn` 타이밍 이슈에 의존하여 로직(입력 활성화 등)이 분기되고 있습니다. 이는 네트워크 환경에 따라 스폰 시점에 비정상적인 상태(입력 불가 등)가 발생할 수 있습니다.
- **상태 관리:** `_isRespawn` 플래그를 외부에서 setter로 설정한 뒤 스폰 로직에 활용하고 있는데, 이는 상태 변경의 추적을 어렵게 만듭니다.
- **성능:** `IReadOnlyList<IBehaviourWithInput>` 내의 모든 비헤이비어들에 매번 메시지(`StartStageMessage`)를 구독/해제하는 과정이 반복되고 있는데, 이 리스트가 동적으로 변하거나 재구성될 경우 메모리 누수의 위험이 있습니다.

## 3. 제안 (Proposal)
- **스폰 이벤트 설계:** `NetworkBehaviour`의 로직 분기(Spawn/PostSpawn 등)를 최소화하고, 상태 머신을 도입하여 `Active`/`Inactive` 상태를 명시적으로 관리하십시오.
- **입력 시스템 분리:** 입력을 활성화/비활성화하는 로직(`EnableInputs`/`DisableInputs`)을 `PlayerSyncer`가 직접 루프를 돌며 제어하지 말고, 입력 핸들러(`PlayerInput`)와 비헤이비어들 간의 메시지 구독 구조를 더 정교하게 다듬어 `PlayerSyncer`의 책임을 줄이십시오.
- **데이터 흐름 명시:** `_isRespawn` 같은 플래그 대신, `PlayerSpawnPacket`과 같은 전용 데이터 패킷을 활용하여 스폰 시점에 필요한 상태를 명확히 전달하도록 구조를 개선하십시오.
