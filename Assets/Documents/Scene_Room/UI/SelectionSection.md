# SelectionSection.cs 기술 문서

**개요**
`SelectionSection`은 로비 룸 씬에서 게임 모드(Coop/Comp) 전환 및 스테이지를 선택하는 UI 섹션 컴포넌트입니다. `SectionBase`를 상속받아 화면 전환 시 제어를 수행합니다.

**필드 (Fields)**
- `modeBtn`: 게임 모드 전환 버튼.
- `prevBtn`, `nextBtn`: 스테이지 선택 버튼.
- `stageThumbnailBoard`: 스테이지 썸네일 보드 UI.
- `_viewData`: 테마 색상 정보를 담은 `RoomViewData`.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 서비스들을 초기화하고 모드 전환 요청/응답 메시지를 설정합니다.
- **`Show` / `Hide`**: 버튼 리스너를 설정/해제하고 UI 요소를 관리합니다.
- **`InitElements`**: 모드, 선택된 스테이지 인덱스, 호스트 여부에 따라 UI 요소를 초기화합니다.
- **`OnClickMode`**: 게임 모드 전환 요청(`SwitchModeRequest`)을 발행합니다.
- **`UpdateModeTheme`**: 모드 변경 메시지(`SwitchModeRespond`) 수신 시 테마를 갱신합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 객체 및 타입들입니다.*
- **SectionBase (Class)**: UI 섹션 기반 클래스.
- **RoomViewData (ScriptableObject)**: UI 데이터 관리.
- **StageMode (Enum)**: 게임 모드 정의.
- **다양한 메시지 클래스 (Struct/Class)**: 통신용 메시지.
