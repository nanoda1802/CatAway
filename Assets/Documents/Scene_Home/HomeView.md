# HomeView.cs 기술 문서

**개요**
`HomeView`는 홈 씬의 UI 화면을 관리하는 컴포넌트입니다. 방 생성 및 참가 버튼 이벤트를 처리하고, 사용자에게 팝업 및 다이얼로그를 표시하며, 주기적으로 UI 요소를 애니메이션(Shake) 효과를 줍니다.

**필드 (Fields)**
- `createRoomBtn`, `joinRoomBtn`: 방 생성 및 참가 버튼.
- `createBtnRectTr`, `joinBtnRectTr`: 버튼의 애니메이션 대상인 `RectTransform`.
- `_tweenHandler`: 애니메이션 효과를 담당하는 서비스.
- `_data`: UI 구성 데이터(`HomeViewData`).

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 UI 서비스와 메시지 발행기를 초기화합니다.
- **`OnEnable` / `OnDisable`**: 버튼 리스너를 설정/해제하고 애니메이션을 시작합니다.
- **`ShakeButton`**: 주기적으로 생성 또는 참가 버튼에 애니메이션(Shake) 효과를 부여합니다.
- **`OnCreate` / `OnJoin`**: 버튼 클릭 시 방 생성 요청을 발행하거나, 방 참가 입력 다이얼로그를 띄우기 위한 메시지를 발행합니다.
- **`SendDialogMessage` / `SendPopUpMessage`**: 사용자 인터랙션을 위한 UI 팝업 메시지를 발행합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 객체 및 타입들입니다.*
- **HomeViewData (ScriptableObject)**: UI 설정 및 애니메이션 데이터.
- **CreateRoomRequest, PopUpMessage, DialogMessage (Struct/Class)**: UI 및 네트워크 통신용 메시지.
- **DialogButtonType (Enum)**: 다이얼로그 버튼 구성.
