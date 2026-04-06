# ResultScope.cs 기술 문서

**개요**
`ResultScope`는 결과 씬의 의존성 관리 범위를 정의하는 `LifetimeScope`입니다. 씬 내 서비스, 데이터 및 UI 컴포넌트를 등록하고 메시지 브로커를 설정합니다.

**필드 (Fields)**
- `viewRectTr`: UI 화면의 부모 `RectTransform`.
- `memberPrefab`: 결과 씬 멤버 프리팹(`ResultMember`).
- `viewData`: 결과 씬 UI 데이터(`ResultViewData`).
- `resultMemberCardPrefab`: 멤버 정보 UI 카드 프리팹(`ResultMemberCard`).

**주요 메서드 (Methods)**
- **`Configure(IContainerBuilder builder)`**: DI 컨테이너를 설정합니다.
    - **`Register<IButtonAction<...>>`**: 퀵 메뉴 버튼 액션(`Customize`, `Skip`, `Settings`, `Leave`)을 등록합니다.
    - **`Register<ResultMemberCardProvider>`**: 멤버 카드 제공 서비스를 등록합니다.
    - **`UseEntryPoints`**: 결과 씬 초기화 서비스(`ResultInitiator`)를 등록합니다.
    - **`UseComponents`**: UI 구성 요소들을 컨테이너에 주입합니다.
    - **`RegisterMessageBroker<T>`**: 결과 씬 관련 통신 및 UI 상태 관리를 위한 다양한 메시지 브로커들을 설정합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 객체 및 타입들입니다.*
- **ResultMember (Class)**: 씬 멤버 관리 클래스.
- **ResultViewData (ScriptableObject)**: 결과 씬 UI 데이터.
- **ResultMemberCard (Class)**: 멤버 카드 UI 클래스.
- **다양한 메시지 및 액션 클래스들 (Struct/Class/Interface)**: 씬 내 통신 및 UI 동작 정의.
