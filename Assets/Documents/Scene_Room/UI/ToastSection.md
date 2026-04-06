# ToastSection.cs 기술 문서

**개요**
`ToastSection`은 로비 룸 씬에서 사용자에게 일시적인 알림 메시지(Toast)를 표시하는 UI 컴포넌트입니다. `SectionBase`를 상속받아 화면 제어를 수행합니다.

**필드 (Fields)**
- `toastTxt`: 알림 메시지를 표시하는 `TextMeshProUGUI`.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 알림 메시지(`RoomToastMessage`)를 구독하고 메시지 도착 시 `DisplayToast`를 호출합니다.
- **`DisplayToast`**: 알림 메시지 수신 시 텍스트를 설정하고 알림 UI를 표시합니다.
- **`Show`**: 알림 UI를 활성화하고 3초간 대기 후 자동으로 `Hide`를 호출합니다.
- **`Hide`**: 알림 UI를 비활성화합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 객체 및 타입들입니다.*
- **SectionBase (Class)**: UI 섹션 기반 클래스.
- **RoomToastMessage (Struct/Class)**: UI 알림용 메시지.
