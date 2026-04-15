`C:\Users\home\Documents\GitHub\PartTimeCat\Assets\_Scripts\Home`

# Home: 초기 진입 및 네트워크 접속

본 문서는 프로젝트의 `Home` 씬에서 발생하는 초기 진입, 네트워크 접속, 그리고 아바타 관리 로직에 대해 설명합니다.

## 1. Home 씬 개요
`Home` 씬은 사용자가 게임에 처음 접속했을 때 마주하는 초기 화면입니다. 이 씬에서는 주로 다음 기능들을 제공합니다:
- 방 생성 및 참여를 통한 네트워크 세션 접속
- 플레이어 아바타 및 닉네임 설정
- 퀵 메뉴를 통한 설정 및 게임 종료

## 2. HomeScope.cs
`Home` 씬의 VContainer Lifetime Scope로, 해당 씬에서 필요한 의존성을 주입하고 관리합니다.

### 주요 등록 요소:
- **데이터:** `HomeViewData` (씬의 View 관련 데이터를 담을 것으로 예상)
- **QuickMenu 액션:** `RenameAction`, `CustomizeAction`, `TutorialAction`, `SettingsAction`, `ExitAction` 등 `QuickMenu` 버튼 클릭 시 실행될 액션들을 `IButtonAction` 인터페이스 구현체로 등록합니다.
- **메시지 브로커 (MessagePipe):**
    - `CreateRoomRequest`, `JoinRoomRequest`: 방 생성 및 참여 요청 메시지.
    - `PopUpMessage`, `PopDownMessage`, `DialogMessage`: 공통 팝업 시스템 제어 메시지.
    - `AvatarMessage`: 아바타 변경 요청 메시지.

## 3. RoomConnector.cs
`Home` 씬의 핵심 네트워크 접속 관리 컴포넌트입니다. 플레이어의 방 생성 및 참여 요청을 처리하고, 네트워크 연결 상태를 관리합니다.

### 주요 기능:
- **생성자 주입:** `NetworkManager`, `UnityTransport`, `RoomStatus`, `MessagePipe` 퍼블리셔/서브스크라이버 등을 주입받습니다.
- **방 생성 (`CreateRoom`):**
    - `CreateRoomRequest` 메시지를 구독하여 요청을 처리합니다.
    - 임시로 룸 코드를 생성하고 (`_roomStatus.Code`), `NetworkManager.StartHost()`를 호출하여 호스트를 시작합니다.
    - (현재) 릴레이 서버와의 연동은 `UniTask.Delay`를 통해 시뮬레이션 중입니다.
- **방 참여 (`JoinRoom`):**
    - `JoinRoomRequest` 메시지를 구독하여 요청을 처리합니다.
    - `UnityTransport.SetConnectionData()`를 사용하여 연결 데이터를 설정합니다.
    - `NetworkManager.StartClient()`를 호출하여 클라이언트로 접속을 시도합니다.
    - (현재) 릴레이 서버와의 연동은 `UniTask.Delay`를 통해 시뮬레이션 중입니다.
- **연결 이벤트 처리 (`OnConnection`):**
    - `NetworkManager.OnConnectionEvent`를 구독하여 클라이언트 연결 및 연결 해제 이벤트를 처리합니다.
    - 접속 실패 시 `NetworkManager.DisconnectReason`을 포함한 `DialogPop` 메시지를 발행하여 사용자에게 피드백을 제공합니다.
- **씬 전환:** 호스트 시작 시 `OnNetworkSpawn()`에서 `LoadSceneMessage`를 발행하여 `Room` 씬으로 전환을 요청합니다.

## 4. HomeAvatar.cs
`Home` 씬에서 플레이어의 아바타를 표시하고 변경을 처리하는 컴포넌트입니다.

### 주요 기능:
- **생성자 주입:** `AvatarData` (아바타 정보 SO)와 `PlayerStatus` (플레이어 상태)를 주입받습니다.
- **아바타 변경:** `AvatarMessage` 메시지를 구독하여 아바타 변경 요청을 수신합니다.
- `AvatarData.ChangeAvatar()` 메서드를 호출하여 `SkinnedMeshRenderer`의 아바타 모델을 업데이트하고, `PlayerStatus`에 변경된 아바타 인덱스를 반영합니다.

## 5. Home UI 요소 (HomeView.cs)

`HomeView.cs`는 `Home` 씬의 메인 UI를 담당하며, 사용자 입력을 받아 네트워크 접속 로직을 트리거합니다.

### 주요 기능:
- **방 생성/참여 버튼:** `createRoomBtn`과 `joinRoomBtn`을 통해 사용자가 방을 생성하거나 기존 방에 참여할 수 있는 UI를 제공합니다.
- **버튼 애니메이션:** `TweenHandler`를 활용하여 버튼에 주기적인 흔들기 애니메이션을 적용하여 시각적 흥미를 유발합니다. (`ShakeButton` 코루틴)
- **방 생성 요청:** `createRoomBtn` 클릭 시 `CreateRoomRequest` 메시지를 발행하고, `DialogPop`을 이용한 대기 다이얼로그를 표시합니다.
- **방 참여 요청:** `joinRoomBtn` 클릭 시 `DialogPop`을 통해 룸 코드를 입력받는 다이얼로그를 표시합니다.
- **팝업 연동:** `PopUpMessage`와 `DialogMessage`를 발행하여 `Shared`의 팝업 시스템(`DialogPop`)과 연동하여 사용자에게 정보 입력 및 피드백을 제공합니다.

