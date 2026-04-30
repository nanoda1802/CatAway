`C:\Users\home\Documents\GitHub\PartTimeCat\Assets\_Scripts\Result`

# Result Scene 기술 문서

## 1. 개요 (Overview)
Result 씬은 게임 스테이지가 종료된 후 최종 결과를 정산하고, 플레이어들에게 결과를 시각적으로 보여주는 씬입니다. 각 플레이어의 최종 팀 정보, 승패 여부, 에이스 플레이어 정보 등을 동기화하여 표시하며, 플레이어들이 결과를 확인한 후 로비로 돌아가거나 게임을 종료할 수 있도록 합니다.

## 2. Scope (`ResultScope.cs`)
VContainer의 `LifetimeScope`를 상속받아 Result 씬의 의존성을 관리합니다.

- **의존성 주입 (VContainer):**
  - `ResultInitiator`를 `EntryPoint`로 등록하여 씬 로드 완료 시 초기화 로직을 수행합니다.
  - `ResultMemberCardProvider`를 싱글톤으로 등록하여 플레이어 카드 UI 관리를 담당합니다.
  - 각종 메시지 브로커(`StartResultMessage`, `ResultBoardMessage`, `SkipRequest` 등)를 등록하여 모듈 간 비동기 통신을 지원합니다.
  - `ResultMember` 프리팹, `ResultViewData`, `ResultMemberCard` 프리팹 등을 인스턴스로 등록합니다.
- **메시지 브로커 (MessagePipe):**
  - 결과 시작, 타이머, 스킵 요청/응답, 카드 UI 애니메이션 제어 등 다양한 이벤트를 메시지 기반으로 처리합니다.

## 3. 핵심 로직 컴포넌트

### 3.1 `ResultInitiator`
- **역할:** 씬 로드 완료 후 결과 처리 프로세스를 시작합니다.
- **주요 기능:**
  - `NetworkManager.SceneManager.OnLoadEventCompleted` 이벤트를 구독하여 모든 클라이언트가 씬 로드를 마쳤는지 확인합니다.
  - 모든 클라이언트 로드 완료 시, 서버에서 `StartResultMessage`를 발행하여 결과 산출 및 UI 표시를 시작합니다.

### 3.2 `ResultMemberSyncer`
- **역할:** 결과 씬에서 각 플레이어를 나타내는 `ResultMember` 객체를 서버에서 생성 및 동기화합니다.
- **주요 기능:**
  - 서버에서 `RoomStatus`의 `ActiveMembers` 정보를 바탕으로 플레이어 오브젝트를 스폰합니다.
  - `ResultMemberPrefabHandler`를 `PrefabHandler`에 등록하여 `NetworkObject`의 생성/파괴를 처리합니다.
  - 플레이어의 위치와 회전을 계산하여 결과 씬 내에서의 배치를 관리합니다.
  - 플레이어 연결 종료 시 리스트에서 제거하고 배치를 갱신합니다.

### 3.3 `ResultMember`
- **역할:** 결과 씬에 나타나는 각 플레이어 캐릭터의 상태(아바타, 닉네임, 팀 등)를 네트워크로 동기화하고 시각화합니다.
- **주요 기능:**
  - `NetworkVariable`을 통해 `AvatarIndex`, `Nickname`, `Team`, `AceId`를 모든 클라이언트에 동기화합니다.
  - `OnNetworkPostSpawn` 시 서버로부터 받은 데이터를 바탕으로 아바타를 설정하고, 자신의 결과를 보여주는 카드 UI 메시지(`ShowResultMemberCardMessage`)를 발행합니다.
  - 데이터 변경 시(`OnAvatarIndexChanged`, `OnNicknameChanged`) 관련 UI 메시지를 발행하여 UI를 업데이트합니다.
  - `Rpc`를 통해 카드 UI의 이동, 숨김 처리를 모든 클라이언트에게 동기화합니다.

### 3.4 `ResultMemberPrefabHandler`
- **역할:** `NetworkManager`가 `ResultMember` 객체를 스폰할 때 사용할 커스텀 핸들러입니다.
- **주요 기능:**
  - `ResultMemberSyncer`를 통해 프리팹을 인스턴스화하고 `NetworkObject`를 반환합니다.

## 4. UI 요소 (Result/UI)

### 4.1 `ResultView`
- **역할:** Result 씬의 메인 UI를 관리하고, 플레이어 카드 UI 및 타이머 등을 업데이트합니다.
- **주요 기능:**
  - `MessagePipe`를 통해 카드 생성/이동/삭제 메시지를 받아 `ResultMemberCardProvider`를 통해 UI를 갱신합니다.
  - 결과 씬 타이머 업데이트 메시지를 받아 UI 텍스트에 표시하고, 종료 시간이 임박하면 색상을 변경합니다.

### 4.2 `ResultBoard` & `ResultBoardPresenter`
- **역할:** 각 팀의 최종 점수, 콤보, 납품률 등 정산 결과를 보여줍니다.
- **주요 기능:**
  - `ResultBoardPresenter`는 서버에서 `StageResultInfo`를 바탕으로 팀별 결과 메시지(`ResultBoardMessage`)를 발행합니다.
  - `ResultBoard`는 결과 메시지를 받아 트윈 애니메이션을 사용하여 팀 점수와 승패 여부를 순차적으로 표시합니다.

### 4.3 `ResultMemberCard` & `ResultMemberCardProvider`
- **역할:** 화면상에 위치한 캐릭터 위에 플레이어 정보(이름, 팀 색상, 에이스 여부)를 표시하는 카드 UI입니다.
- **주요 기능:**
  - `ResultMemberCardProvider`는 `ObjectPool`을 사용하여 카드 프리팹을 생성하고 관리합니다.
  - `ResultMemberCard`는 해당 캐릭터의 위치를 카메라 변환을 통해 스크린 좌표로 변환하여 배치합니다.

### 4.4 `ResultTimerPresenter` & `SkipVotePresenter`
- **역할:** 씬 유지 시간과 플레이어들의 스킵 투표를 관리합니다.
- **주요 기능:**
  - `ResultTimerPresenter`는 `NetworkUpdateSystem`을 사용하여 동기화된 타이머를 카운트다운하고 시간 종료 시 로비 씬으로 전환합니다.
  - `SkipVotePresenter`는 플레이어들의 스킵 투표를 관리하며, 모두 투표 완료 시 스킵을 실행합니다.
  - `SkipVoteBoard`는 현재 투표 상태를 아이콘으로 화면에 표시합니다.

## 5. 데이터 및 메시지 정의

### 5.1 데이터 구조체 (Result/_Data)
- **ResultViewData:** 결과 씬의 타이머 설정, 투표 아이콘 리소스, 플레이어 카드 관련 상수(간격, 최대 개수 등)를 관리하는 `ScriptableObject`입니다.
- **SkipVoteStatus:** 현재 투표 수와 총 투표권자 수를 포함하여 투표 현황을 네트워크로 동기화합니다.

### 5.2 메시지 구조체 (Result/_Messages)
- **ResultBoardMessage:** 팀 결과 데이터(승패 여부, 점수, 콤보, 납품률)를 담아 UI로 전달합니다.
- **ShowResultMemberCardMessage:** 결과 씬에서 특정 플레이어의 정보 카드 UI를 표시하기 위한 데이터를 담습니다.
- **SkipRequest / SkipRespond:** 스킵 투표 요청 및 그 결과(투표수)를 전달합니다.
- **StartResultMessage:** 결과 씬의 시작을 클라이언트들에게 알립니다.
