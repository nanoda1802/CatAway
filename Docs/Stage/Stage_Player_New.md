`C:\Users\home\Documents\GitHub\PartTimeCat\Assets\_Scripts\Stage\Player`

# Stage 모듈: Player 관련 컴포넌트

이 문서는 `Assets/_Scripts/Stage/Player/` 하위의 플레이어 관련 로직, 상태 관리 및 상호작용 컴포넌트에 대해 설명합니다.

## 1. 개요 (Overview)
플레이어 객체의 이동, 물건 운반, 상호작용, 충돌 처리 등 게임플레이 핵심 로직을 담당합니다. `VContainer`를 통해 의존성을 주입받으며, `Unity.Netcode`를 기반으로 네트워크 동기화를 수행합니다.

## 2. Scope (`PlayerScope.cs`)
`LifetimeScope`를 상속받아 플레이어 객체의 생명주기와 의존성을 관리합니다.

*   **등록된 싱글톤:** `MoveStatus`, `DetectStatus`, `CarryStatus`, `InteractStatus`
*   **주입 데이터:** `PlayerData` (설정값), `AvatarData` (아바타 에셋 정보)
*   **컴포넌트 구성:** `Rigidbody`, `SkinnedMeshRenderer`, `Animator`, `Transform` (탐지용), `PlayerSyncer`, `MovementBehaviour`, `InteractionBehaviour`, `CarrierBehaviour`, `CollisionBehaviour`, `EmotionBehaviour` 등을 컴포넌트 빌더를 통해 주입.

## 3. 핵심 로직 컴포넌트

### 3.1 `PlayerSyncer.cs`
네트워크 동기화의 중심 컴포넌트입니다. `NetworkBehaviour`를 상속받아 클라이언트 간 아바타 인덱스 동기화 및 입력 활성화/비활성화를 관리합니다.
*   **주요 기능:** 아바타 외형 동기화 (`NetworkVariable` 사용), 게임 시작/종료 시 입력(`PlayerInput`) 제어, 서버에서의 플레이어 데스폰 처리.

### 3.2 `MovementBehaviour.cs`
플레이어의 이동 및 대시 기능을 담당합니다. `INetworkUpdateSystem`을 사용하여 `FixedUpdate`에서 물리 연산을 수행합니다.
*   **주요 기능:** 입력값 기반 이동 및 회전, `UniTask`를 활용한 대시 로직 및 VFX/SFX 처리.

### 3.3 `CarrierBehaviour.cs`
물건을 들거나(Pick), 놓거나(Drop), 던지는(Throw) 상호작용을 담당합니다. `AttachableNode`를 상속받아 `NetworkObject` 간 부착 처리를 수행합니다.
*   **주요 기능:** 인접한 아이템/테이블 탐지, 아이템 부착, RPC를 통한 서버 사이드 동작 요청.

### 3.4 `InteractionBehaviour.cs`
테이블과의 상호작용(예: 요리, 청소)을 담당합니다.
*   **주요 기능:** 상호작용 가능 여부 확인, 상호작용 상태 동기화 및 애니메이션 처리, RPC를 통한 상호작용 시작/취소/종료 관리.

### 3.5 `CollisionBehaviour.cs`
플레이어와 물건(Ingredient) 간 충돌 처리를 담당합니다.
*   **주요 기능:** 던져진 아이템에 맞았을 때 넉백 처리(서버 연산 후 클라이언트 넉백 RPC).

## 4. 데이터 및 상태 정의

### 4.1 `PlayerData.cs` (ScriptableObject)
플레이어 이동 속도, 대시 시간, 상호작용 간격, 레이어 마스크 등 플레이어 동작과 관련된 모든 설정값들을 포함합니다.

### 4.2 상태 관리 (Status)
각 기능별로 데이터를 캡슐화하여 상태를 관리합니다.
*   `MoveStatus`: 이동 방향, 회전, 대시 쿨타임 및 물리 제어 상태 관리.
*   `DetectStatus`: `Physics.OverlapBox` 및 `Raycast`를 통해 주변 아이템 및 테이블 탐지.
*   `CarryStatus`: 현재 운반 중인 물건 관리 및 운반 쿨타임 관리.
*   `InteractStatus`: 상호작용 애니메이션 해시 관리 및 현재 상호작용 중인 대상 관리.

---
*참고: 플레이어 입력 제어는 `PlayerInput` 클래스와 `IBehaviourWithInput` 인터페이스를 통해 일괄 관리됩니다.*

## Feedback
- 점검 결과: `Assets/_Scripts/Stage/Player` 하위 스크립트 구조와 문서 내용이 일치함을 확인했습니다. 플레이어의 핵심 로직 컴포넌트(`PlayerSyncer`, `MovementBehaviour`, `CarrierBehaviour`, `InteractionBehaviour`, `CollisionBehaviour`)와 VContainer 스코프(`PlayerScope`), 그리고 상태 관리(`Status` 하위) 구조가 정확히 기술되어 있습니다. 의존성 주입 구조 및 네트워크 동기화 방식에 대한 설명도 실제 코드와 일치합니다. 추가 수정 사항 없습니다.
