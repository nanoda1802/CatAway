# RoomMemberCard.cs 기술 문서

**개요**
`RoomMemberCard`는 로비 룸 씬에서 개별 플레이어의 이름과 준비 상태 아이콘을 표시하는 UI 카드 컴포넌트입니다. 플레이어의 위치를 따라가며 화면에 표시됩니다.

**필드 (Fields)**
- `cardRectTr`: 카드 UI의 `RectTransform`.
- `iconImg`: 멤버 상태 아이콘(`MemberIconType` 반영).
- `nameTxt`: 멤버 이름 표시 `TextMeshProUGUI`.
- `_data`: 아이콘 정보 및 오프셋 데이터를 담은 `RoomViewData`.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 UI 설정 데이터와 캔버스 영역(`RectTransform`)을 초기화합니다.
- **`Show` / `Hide`**: 카드 UI의 활성화/비활성화.
- **`UpdatePosition`**: 멤버의 월드 공간 좌표를 계산하여 UI 화면상의 올바른 위치(`anchoredPosition`)로 갱신합니다.
- **`SetIcon`**: `MemberIconType`에 따라 아이콘과 색상을 설정합니다.
- **`SetName`**: 플레이어 이름을 갱신합니다.
- **`SwitchReadyIcon`**: 준비 상태에 따라 아이콘을 즉시 전환합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 및 타입들입니다.*
- **RoomViewData (ScriptableObject)**: 카드 UI 테마 및 아이콘 데이터.
- **MemberIconType (Enum)**: 멤버 상태 아이콘 타입(Host, Ready, NonReady).
