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


---
## Feedback
- **점검 결과:** 문서에 기술된 경로(`Assets/_Scripts/Shared/Infrastructure/`)가 실제 프로젝트 구조(`Assets/_Scripts/Shared/`)와 상이합니다. 인프라 관련 핵심 클래스(`RootScope`, `SceneChanger`, `ApprovalManager`)들이 최상위 Shared 폴더에 위치하고 있습니다.
- **메시지 시스템:** `MessagePipe`를 사용하는 것은 맞으나, `LoadSceneMessage`, `AvatarMessage`, `RenameMessage` 외에도 다양한 메시지들이 `Assets/_Scripts/Shared/_Messages/`에 존재하므로, 문서 보완이 필요합니다.
- **권장 사항:** 문서 상단의 경로를 실제 위치(`Assets/_Scripts/Shared/`)로 수정하고, 메시지 시스템 섹션에 다양한 메시지 종류에 대한 포괄적인 설명을 추가하는 것을 권장합니다.
