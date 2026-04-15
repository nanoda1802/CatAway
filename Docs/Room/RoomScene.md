`C:\Users\home\Documents\GitHub\PartTimeCat\Assets\_Scripts\Room`

# Room: 대기실 및 스테이지 설정

본 문서는 프로젝트의 `Room` 씬에서 발생하는 대기실 기능, 스테이지 설정, 멤버 관리 및 네트워크 동기화 로직에 대해 설명합니다.

## 1. Room 씬 개요
`Room` 씬은 플레이어들이 게임 시작 전에 모여 대기하고, 스테이지를 선택하며, 팀을 구성하는 공간입니다. 주로 다음과 같은 기능들을 제공합니다:
- 룸 코드 표시 및 공유
- 룸 멤버들의 아바타 및 정보 표시
- 스테이지 모드(협동/경쟁) 및 스테이지 선택
- 게임 시작 준비 및 시작
- 퀵 메뉴를 통한 설정 및 룸 나가기

## 2. RoomScope.cs
`Room` 씬의 VContainer Lifetime Scope로, 해당 씬에서 필요한 의존성을 주입하고 관리합니다.

### 주요 등록 요소:
- **Singletons:** `RoomMemberCardProvider`, `PointSwapper`
- **인스턴스:** `memberPrefab`, `roomMemberCardPrefab`, `viewData` (씬의 View 관련 데이터)
- **QuickMenu 액션:** `RenameAction`, `CustomizeAction`, `TutorialAction`, `SettingsAction`, `LeaveAction` 등 `QuickMenu` 버튼 클릭 시 실행될 액션들을 `IButtonAction` 인터페이스 구현체로 등록합니다.
- **메시지 브로커 (MessagePipe):**
    - `InitRoomMessage`, `LeaveRoomMessage`, `RoomToastMessage`: 룸 초기화, 나가기, 토스트 알림.
    - `SwitchStartMessage`: 게임 시작 가능 여부 알림.
    - `SwitchModeRequest`, `SwitchModeRespond`: 스테이지 모드 전환 요청/응답.
    - `SwitchReadyRequest`, `SwitchReadyRespond`: 준비 상태 전환 요청/응답.
    - `SelectStageRequest`, `SelectStageRespond`: 스테이지 선택 요청/응답.
    - `ShowRoomMemberCardMessage`, `HideMemberCardMessage`, `MoveMemberCardMessage`, `UpdateMemberNameMessage`: 룸 멤버 카드 UI 제어 메시지.

## 3. 룸 상태 동기화 (RoomSyncer.cs)
`RoomSyncer`는 `NetworkBehaviour`를 상속하여 룸의 전역 상태를 네트워크를 통해 동기화하고 관리합니다.

### 주요 기능:
- **네트워크 변수:** `_sharedCode` (룸 코드)와 `_sharedStageInfo` (선택된 스테이지 정보)를 `NetworkVariable`로 선언하여 모든 클라이언트 간에 자동으로 동기화됩니다.
- **생성자 주입:** `StageListData`, `RoomStatus`, `MessagePipe` 퍼블리셔/서브스크라이버를 주입받습니다.
- **스테이지 모드/선택 요청 처리:** `SwitchModeRequest`와 `SelectStageRequest` 메시지를 구독하여 서버에서 스테이지 모드 전환 및 스테이지 선택 로직을 수행합니다.
- **변경 이벤트 발행:** `_sharedStageInfo.OnValueChanged` 이벤트를 통해 스테이지 모드나 인덱스가 변경될 때 `SwitchModeRespond` 또는 `SelectStageRespond` 메시지를 발행하여 UI 업데이트 등을 트리거합니다.
- **룸 초기화:** `OnNetworkSpawn()` 시 호스트는 `_roomStatus`의 코드를 `_sharedCode`에 할당하고, 모든 클라이언트에게 `InitRoomMessage`를 발행하여 룸 정보를 초기화합니다.

## 4. 룸 멤버 관리 (RoomMemberSyncer.cs)
`RoomMemberSyncer`는 룸 멤버들의 스폰, 위치 관리, 네트워크 접속/해제 처리를 담당하는 핵심 컴포넌트입니다.

### 주요 기능:
- **멤버 스폰 및 위치 지정:** `MemberPoint` 배열을 사용하여 각 멤버의 스폰 위치와 회전을 관리합니다.
- **Prefab Handler:** `NetworkManager.PrefabHandler`를 통해 `RoomMember` 프리팹의 네트워크 스폰 및 관리를 최적화합니다.
- **연결 이벤트 처리:** `NetworkManager.OnConnectionEvent`를 구독하여 클라이언트가 연결되거나 해제될 때 `AddMember` 또는 `RemoveTargetMember`를 호출하여 룸 멤버를 추가/제거하고, `RoomStatus`에 반영합니다.
- **룸 로드 완료 처리:** 서버의 `OnRoomLoadComplete` 이벤트에서 활성 멤버들을 기반으로 `RoomMember` 오브젝트를 스폰합니다.
- **멤버 위치 교체:** `SwapMember` 기능을 통해 서버에서 룸 멤버들의 위치를 교체하고, 해당 멤버들의 준비 상태를 초기화하도록 RPC를 호출합니다.
- **준비 상태 초기화:** `SwitchModeRequest` 메시지를 구독하여 스테이지 모드 변경 시 호스트가 아닌 멤버들의 준비 상태를 초기화합니다.
- **게임 시작 가능 여부 (`CanStartStage`):** 모든 멤버가 준비되었는지, 그리고 경쟁 모드일 경우 각 팀에 최소 한 명의 멤버가 있는지 확인하여 게임 시작 가능 여부를 판단하고 `SwitchStartMessage`를 발행합니다.

## 6. Room UI 요소

`Room` 씬은 여러 UI 섹션으로 구성되어 있으며, 각 섹션은 `SectionBase.cs`를 상속하여 Show/Hide 애니메이션을 공통으로 관리합니다.

### RoomView.cs
- **역할:** `Room` 씬의 모든 UI 섹션을 총괄하고, 룸 멤버 카드 UI의 생명주기와 업데이트를 관리합니다.
- **주요 기능:**
    - `SectionBase` 배열을 통해 `CodeSection`, `ButtonSection`, `SelectionSection`, `ToastSection` 등을 관리합니다.
    - `InitRoomMessage`를 수신하여 각 섹션을 초기화합니다.
    - `RoomMemberCardProvider`를 통해 `RoomMemberCard`를 얻고 해제하며, `ShowRoomMemberCardMessage`, `HideMemberCardMessage`, `MoveMemberCardMessage`, `UpdateMemberNameMessage`를 구독하여 룸 멤버 카드 UI를 동적으로 업데이트합니다.
    - `UniTask`를 이용한 비동기 섹션 Show/Hide 애니메이션을 제공합니다.

### CodeSection.cs
- **역할:** 현재 룸의 입장 코드를 표시하고, 사용자가 클립보드에 코드를 복사할 수 있도록 합니다.
- **주요 기능:** 룸 코드 표시, 클립보드 복사 버튼, 복사 완료 시 `RoomToastMessage` 발행.

### ButtonSection.cs
- **역할:** 룸 내에서 `준비` 및 `시작` 관련 상호작용 버튼을 관리합니다.
- **주요 기능:**
    - `준비` 버튼: 클라이언트가 자신의 준비 상태를 전환할 수 있도록 `SwitchReadyRequest` 메시지를 발행합니다.
    - `시작` 버튼: 호스트만 활성화되며, 모든 클라이언트가 준비되고 팀 구성이 완료되었을 때 `Stage` 씬으로 전환하는 `LoadSceneMessage`를 발행합니다.
    - 스테이지 모드에 따른 버튼 테마 변경 및 `Start` 버튼의 활성화/비활성화 상태를 관리합니다.

### SelectionSection.cs
- **역할:** 스테이지 모드(협동/경쟁) 전환 및 스테이지 선택 UI를 제공합니다.
- **주요 기능:**
    - **모드 전환:** 호스트만 `modeBtn`을 통해 스테이지 모드를 전환할 수 있으며, `SwitchModeRequest` 메시지를 발행합니다.
    - **스테이지 선택:** `prevBtn`, `nextBtn`을 통해 `StageThumbnailBoard`의 스테이지 선택 기능을 호출합니다.
    - 호스트에게만 모드 전환 및 스테이지 선택 버튼이 활성화됩니다.

### StageThumbnailBoard.cs
- **역할:** 선택 가능한 스테이지의 썸네일을 시각적으로 표시하고, 사용자 스와이프 입력을 통해 스테이지를 선택할 수 있도록 합니다.
- **주요 기능:**
    - `IDragHandler` 인터페이스를 구현하여 마우스/터치 드래그를 감지하고 스테이지 스와이프를 처리합니다.
    - `StageListData`로부터 스테이지 썸네일 이미지를 로드하여 표시합니다.
    - 좌우 스와이프 또는 버튼 클릭 시 `SelectStageRequest` 메시지를 발행하여 스테이지 선택을 요청합니다.

### RoomMemberCard.cs
- **역할:** 룸에 참여한 각 멤버의 정보를 UI 카드 형태로 표시합니다.
- **주요 기능:**
    - 멤버의 월드 좌표를 UI 캔버스 좌표로 변환하여 아바타 위에 오버레이합니다.
    - 호스트 아이콘, 준비 상태 아이콘(`체크`/`X`), 멤버 닉네임 등을 표시하고 업데이트합니다.

### ToastSection.cs
- **역할:** 룸 내에서 발생하는 간단한 알림 메시지(토스트)를 화면에 표시합니다.
- **주요 기능:** `RoomToastMessage`를 구독하여 메시지를 표시하고, 일정 시간 후 자동으로 사라지게 합니다.
