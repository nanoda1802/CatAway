# ButtonSection.cs 기술 문서

**개요**
`ButtonSection`은 로비 룸 씬에서 플레이어의 준비 상태(`Ready`) 및 방장 전용 스테이지 시작(`Start`) 버튼을 관리하는 UI 컴포넌트입니다. `SectionBase`를 상속받아 화면 전환 시 제어를 수행합니다.

**필드 (Fields)**
- `readyBtn` 등: 준비 버튼 및 관련 UI 요소(이미지, 텍스트).
- `startBtn` 등: 스테이지 시작 버튼 및 관련 UI 요소.
- `_data`: 테마 색상 및 아이콘 정보를 담은 `RoomViewData`.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 UI 서비스 및 메시지 브로커를 초기화하고, 게임 준비/시작/모드 전환 메시지를 구독합니다.
- **`Show` / `Hide`**: 버튼 클릭 리스너를 설정/해제하고 UI 요소를 활성화/비활성화합니다.
- **`ApplyTheme` / `UpdateModeTheme`**: 현재 게임 모드(`StageMode`)에 따라 버튼 테마 색상을 변경합니다.
- **`UpdateReadyButton`**: 준비 상태 변경 메시지(`SwitchReadyRespond`)에 따라 준비 버튼의 아이콘과 상태를 갱신합니다.
- **`UpdateStartButton`**: 시작 가능 여부 메시지(`SwitchStartMessage`)에 따라 시작 버튼의 활성화 상태를 제어합니다.
- **`OnClickReadyBtn` / `OnClickStartBtn`**: 버튼 클릭 이벤트를 처리하여 준비 요청을 발행하거나 스테이지 씬 로드 메시지를 발행합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 객체 및 타입들입니다.*
- **SectionBase (Class)**: UI 섹션의 기반 클래스.
- **RoomViewData (ScriptableObject)**: 버튼 테마 및 상태 데이터.
- **SwitchReadyRespond, SwitchStartMessage, SwitchModeRespond, LoadSceneMessage (Struct/Class)**: 로비 내 UI 상호작용 메시지.
- **StageMode (Enum)**: 게임 모드(Coop, Comp).
