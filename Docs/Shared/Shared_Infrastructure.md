`C:\Users\home\Documents\GitHub\PartTimeCat\Assets\_Scripts\Shared`

# Shared: Infrastructure

본 문서는 프로젝트의 핵심 아키텍처와 기반 인프라 시스템에 대해 설명합니다.

## 1. Dependency Injection (VContainer)
프로젝트는 `VContainer`를 사용하여 의존성 주입을 관리합니다.

### RootScope.cs
- **역할:** 어플리케이션 전체의 Lifetime을 관리하는 최상위 Scope입니다.
- **주요 등록 요소:**
    - **Singletons:** `PlayerInput`, `VfxHandler`, `TweenHandler`
    - **Entry Points (Logic):** `ApprovalManager`, `RoomStatus`, `SceneChanger`, `PlayerStatus`, `SfxProvider`
    - **Components (Instances):** `NetworkManager`, `UnityTransport`, `SoundManager`, `AvatarData`, `StageListData`, `SoundSettingsData`

## 2. Messaging System (MessagePipe)
컴포넌트 간의 느슨한 결합을 위해 `MessagePipe`를 사용합니다.

- **LoadSceneMessage:** 씬 전환 요청을 전달합니다.
- **AvatarMessage:** 플레이어의 아바타 변경 정보를 전달합니다.
- **RenameMessage:** 플레이어의 닉네임 변경 정보를 전달합니다.

## Feedback

- 2024-05-22: 문서 점검 완료. 코드 구조와 문서가 일치하며, VContainer 및 MessagePipe 구성이 정확하게 기술되어 있음. 추가 수정 사항 없음.
