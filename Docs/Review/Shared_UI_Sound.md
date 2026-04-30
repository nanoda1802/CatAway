`C:\Users\home\Documents\GitHub\PartTimeCat\Assets\_Scripts\Shared`

# Shared: Systems (UI & Sound)

본 문서는 프로젝트 전반에서 공통으로 사용되는 UI 시스템과 사운드 시스템에 대해 설명합니다.

## 1. 오디오 시스템 (Sound System)

### SoundManager.cs
- **역할:** BGM 및 SFX의 재생, 정지, 볼륨 페이딩을 총괄하는 중앙 관리자입니다.
- **주요 기능:**
    - **BGM 관리:** `UniTask`를 이용한 비동기 볼륨 페이드 인/아웃 기능을 제공합니다.
    - **SFX 관리:** `SfxProvider`로부터 이벤트를 전달받아 효과음을 재생하며, 현재 재생 중인 모든 SFX 리스트를 관리합니다.
    - **설정 동기화:** `SoundSettingsData`와 연동되어 볼륨 및 뮤트 상태가 실시간으로 반영됩니다.

### SfxProvider.cs / SfxBuilder.cs
- **SfxProvider:** 효과음 방출기(Emitter)의 풀링(Pooling) 및 제공을 담당합니다.
- **SfxBuilder:** 효과음 재생 설정을 위한 빌더 패턴 클래스입니다. 루프 여부, 피치 랜덤화, 재생 위치 등을 체이닝 방식으로 설정할 수 있습니다.

## 2. 공통 UI 시스템

### 팝업 시스템 (Pop System)
- **PopBase.cs:** 모든 팝업의 최상위 추상 클래스로, 열기(`Open`), 닫기(`Close`) 기본 로직과 이벤트를 정의합니다.
- **PopScope.cs:** 팝업 내부의 의존성 주입을 위한 VContainer LifetimeScope입니다.
- **주요 팝업:**
    - `SettingsPop`: 사운드 및 시스템 설정.
    - `CustomizePop`: 아바타 및 플레이어 정보 변경.
    - `DialogPop`: 알림 및 확인 메시지 출력.
    - `TutorialPop`: 게임 방법 설명.


---
## Feedback
- **점검 결과:** 문서에 기재된 `Assets/_Scripts/Shared/UI_Sound/` 경로는 존재하지 않습니다. 사운드 시스템은 `Assets/_Scripts/Shared/Sound/`에, UI 시스템은 `Assets/_Scripts/Shared/UI/` 폴더에 분리되어 있습니다.
- **내용 점검:** 사운드와 UI에 대한 전반적인 기능 설명은 대체로 실제 코드와 일치합니다.
- **권장 사항:** 문서 상단의 경로를 실제 구조에 맞게 `Assets/_Scripts/Shared/`로 통합하거나 각각의 경로(`Sound` 및 `UI`)로 분리하여 기술할 것을 권장합니다.
