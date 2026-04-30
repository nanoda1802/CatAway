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

## Feedback

- 2024-05-22: 문서 점검 완료. 코드 구조와 문서가 일치함. UI(팝업/퀵메뉴) 및 사운드(SoundManager/SfxProvider/SfxBuilder) 관련 로직이 정확하게 기술되어 있음. 추가 수정 사항 없음.
