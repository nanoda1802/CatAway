# RoomView.cs 기술 문서

**개요**
`RoomView`는 룸 씬의 UI 전체를 관리하는 메인 컴포넌트입니다. 여러 섹션(`SectionBase`)을 제어하고, 멤버 카드(`RoomMemberCard`)의 생성 및 업데이트를 조율합니다.

**필드 (Fields)**
- `sections`: 화면 구성을 위한 UI 섹션(`SectionBase`) 배열.
- `_cardProvider`: 멤버 카드를 제공하는 서비스(`RoomMemberCardProvider`).
- `_activeCards`: 현재 활성화된 멤버 카드들을 관리하는 딕셔너리.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 UI 서비스 및 통신 메시지들을 구독합니다.
- **`OnEnable` / `OnDisable`**: 씬 활성화/비활성화 시 UI 섹션을 표시하거나 숨기고, 활성화된 카드들을 정리합니다.
- **`InitSections`**: 초기 룸 정보(`InitRoomMessage`)를 받아 각 섹션을 초기화합니다.
- **`ShowSections` / `HideSections`**: 모든 UI 섹션의 표시 및 숨김을 비동기 처리합니다.
- **`AddMemberCard` / `RemoveMemberCard`**: 멤버 카드 생성/제거 요청을 받아 `_cardProvider`를 통해 풀링된 카드를 관리합니다.
- **`UpdateCardReadyState` / `UpdateCardName` / `UpdateCardPos`**: 메시지를 구독하여 멤버 상태에 맞게 카드의 UI 정보를 갱신합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 및 타입들입니다.*
- **SectionBase (Class)**: UI 섹션의 기반 클래스.
- **RoomMemberCardProvider (Class)**: UI 카드 풀링 서비스.
- **InitRoomMessage, ShowRoomMemberCardMessage 등 (Struct/Class)**: 룸 내 통신 및 UI 상태 메시지.
