# CarrierBehaviour.cs 기술 문서

**개요**
`CarrierBehaviour`는 플레이어가 아이템을 들고, 놓거나, 던지는 등 운반(Carry)과 관련된 상호작용을 처리하는 `NetworkBehaviour`입니다.

**필드 (Fields)**
- `throwPoint`: 아이템을 던질 때 시작 위치와 방향을 나타내는 `Transform`.
- `_sfxList`: 사운드 효과 설정을 담은 `StageSfxListData`.
- `_placementBroker`, `_contactBroker`: 아이템 설치 및 테이블 상호작용을 중개하는 서비스.
- `_carryStatus`, `_detectStatus`: 플레이어의 운반 상태 및 주변 탐지 상태 데이터.
- `_carryAction`, `_throwAction`: 입력 처리용 `InputAction`.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 서비스와 입력을 초기화하고 스테이지 시작/종료 메시지를 구독합니다.
- **`OnAttached` / `OnDetached`**: 아이템이 플레이어에게 부착되거나 분리될 때의 상태 변화(애니메이션, 상태 갱신)를 처리합니다.
- **`BehaveOnEmptyHandRpc` / `BehaveOnCarryingHandRpc`**: 서버에서 실행되며, 빈 손이거나 아이템을 든 상태에 따라 줍기, 설치하기, 던지기 등의 로직을 분기합니다.
- **`ThrowRpc`**: 들고 있는 아이템을 던지는 RPC입니다.
- **`Pick` / `Drop`**: 아이템의 부착 및 분리 로직을 실행합니다.
- **`AssignToBroker`**: 테이블 등과 상호작용할 때 브로커를 통해 처리 결과를 판정하고 효과음을 재생합니다.
- **`SubscribeInputEvents` / `UnsubscribeInputEvents`**: 스테이지 시작/종료 시 입력 이벤트를 관리합니다.
