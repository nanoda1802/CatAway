# RoomScope.cs 기술 문서

**개요**
`RoomScope`는 로비 룸 씬의 의존성 관리 범위를 정의하는 `LifetimeScope`입니다. 씬 내 서비스, 데이터 및 UI 컴포넌트를 등록하고 메시지 브로커를 설정합니다.

**필드 (Fields)**
- `viewRectTr`: UI 화면의 부모 `RectTransform`.
- `memberPrefab`: 씬 멤버 프리팹(`RoomMember`).
- `viewData`: 로비 씬 UI 데이터(`RoomViewData`).
- `roomMemberCardPrefab`: 멤버 정보 UI 카드 프리팹(`RoomMemberCard`).

**주요 메서드 (Methods)**
- **`Configure(IContainerBuilder builder)`**: DI 컨테이너를 설정합니다.
    - **`Register<IButtonAction<...>>`**: 퀵 메뉴 버튼 액션(`Rename`, `Customize`, `Tutorial`, `Settings`, `Leave`)을 등록합니다.
    - **`Register<PointSwapper>` / `Register<RoomMemberCardProvider>`**: 상호작용 및 UI 카드 제공 서비스를 등록합니다.
    - **`UseComponents`**: UI 구성 요소들을 컨테이너에 주입합니다.
    - **`RegisterMessageBroker<T>`**: 룸 씬 내의 통신 및 UI 상태 관리를 위한 다양한 메시지 브로커들을 설정합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 객체 및 타입들입니다.*
- **RoomMember, RoomMemberCard (Class)**: 룸 멤버 및 UI 클래스.
- **RoomViewData (ScriptableObject)**: 로비 씬 UI 데이터.
- **다양한 메시지 및 액션 클래스들 (Struct/Class/Interface)**: 씬 내 통신 및 UI 동작 정의.
