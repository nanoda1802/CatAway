# CodeSection.cs 기술 문서

**개요**
`CodeSection`은 로비 룸 씬에서 방 코드(Room Code)를 표시하고 클립보드에 복사할 수 있는 기능을 제공하는 UI 컴포넌트입니다. `SectionBase`를 상속받아 화면 전환 시 제어를 수행합니다.

**필드 (Fields)**
- `codeTxt`: 방 코드를 표시하는 `TextMeshProUGUI`.
- `copyBtn`: 방 코드를 클립보드에 복사하기 위한 버튼.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 UI 알림 메시지 발행기를 초기화합니다.
- **`Show` / `Hide`**: 버튼 클릭 리스너를 설정/해제하고 UI 요소를 활성화/비활성화합니다.
- **`InitElements`**: 초기 방 코드를 UI에 설정합니다.
- **`OnClickCopy`**: 현재 표시된 방 코드를 시스템 클립보드에 복사하고 사용자에게 복사 완료 메시지(`RoomToastMessage`)를 발행합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 객체 및 타입들입니다.*
- **SectionBase (Class)**: UI 섹션의 기반 클래스.
- **RoomToastMessage (Struct/Class)**: UI 알림용 메시지.
