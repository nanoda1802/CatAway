`C:\Users\home\Documents\GitHub\PartTimeCat\Assets\_Scripts\Shared\_Data` 및 `Assets\_Scripts\Stage\_Data`

# Shared: State & Data

본 문서는 프로젝트의 전역 상태 관리 클래스와 주요 데이터 구조에 대해 설명합니다.

## 1. 전역 상태 관리 (Global State)

### RoomStatus.cs
- **역할:** 현재 룸의 모든 동적 상태를 관리합니다.
- **주요 데이터:**
    - `Members`: 참여 중인 플레이어 정보 (`MemberInfo[4]`).
    - `Code`: 현재 룸의 입장 코드.
    - `SelectedStage`: 선택된 스테이지 정보.
    - `StageResult`: 최근 게임 결과.
- **주요 기능:**
    - 멤버 추가/제거/교체 및 팀 배정(Red/Blue) 로직을 포함합니다.
    - 스테이지 모드(협동/경쟁)에 따른 팀 구성을 자동으로 업데이트합니다.

### PlayerStatus.cs
- **역할:** 로컬 플레이어의 개인 상태를 관리합니다.
- **주요 데이터:** `Nickname`, `AvatarIndex`.
- **특징:** `MessagePipe`를 통해 외부(UI 등)에서 전달되는 `RenameMessage`, `AvatarMessage`를 수신하여 상태를 갱신합니다.

## 2. 네트워크 접속 관리

### ApprovalManager.cs
- **역할:** `Netcode for GameObjects`의 접속 승인 로직을 담당합니다.
- **검증 항목:**
    - 최대 접속 인원 (현재 4명으로 제한).
    - 룸의 가득 참 여부 (`RoomStatus.IsFull`).
    - 중복 클라이언트 ID 확인.
    - (계획) 게임 시작 여부 확인.

## 3. 데이터 구조 (ScriptableObjects)

### StageListData.cs
- **역할:** 프로젝트에 정의된 모든 스테이지 데이터를 보유합니다. (위치: `Assets\_Scripts\Stage\_Data`)
- **주요 기능:**
    - `StageMode`(협동/경쟁)별로 데이터를 분류하여 관리합니다.
    - UI용 썸네일 리스트 반환 및 인덱스 순환 기능을 제공합니다.

### AvatarData / SoundSettingsData
- **AvatarData:** 사용 가능한 아바타 모델 및 프리팹 정보를 정의합니다.
- **SoundSettingsData:** BGM/SFX 볼륨, 뮤트 상태 등을 관리하며 설정 변경 시 이벤트를 발생시킵니다.

## Feedback

- 2024-05-22: 문서 점검 완료. 코드 구조와 문서가 일치함. StageListData.cs의 위치 참조를 수정함 (Shared 디렉토리 내에 존재하지 않음).
