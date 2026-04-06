# HomeScope.cs 기술 문서

**개요**
`HomeScope`는 홈 씬의 의존성 관리 범위를 정의하는 `LifetimeScope`입니다. 씬 내 서비스, 데이터 및 UI 액션을 등록합니다.

**필드 (Fields)**
- `homeViewData`: 홈 씬의 UI 구성 정보를 담은 `HomeViewData`.

**주요 메서드 (Methods)**
- **`Configure(IContainerBuilder builder)`**: DI 컨테이너를 설정합니다.
    - **`RegisterInstance`**: `homeViewData` 인스턴스를 컨테이너에 등록합니다.
    - **`Register<IButtonAction<...>>`**: 퀵 메뉴의 각 버튼 액션들(`Rename`, `Customize`, `Tutorial`, `Settings`, `Exit`)을 스코프 단위로 등록합니다.
    - **`RegisterMessageBroker<T>`**: 씬 내에서 사용되는 메시지 파이프라인 브로커(`CreateRoomRequest`, `JoinRoomRequest`, `PopUpMessage`, `PopDownMessage`, `DialogMessage`, `AvatarMessage`)를 등록합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 객체 및 타입들입니다.*
- **HomeViewData (ScriptableObject)**: 홈 씬 UI 데이터.
- **IButtonAction (Interface/Class)**: 퀵 메뉴 버튼 동작 인터페이스.
- **CreateRoomRequest, JoinRoomRequest, PopUpMessage, PopDownMessage, DialogMessage, AvatarMessage (Struct/Class)**: 홈 씬 내 통신 메시지들.
