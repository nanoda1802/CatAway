# RoomMemberCardProvider.cs 기술 문서

**개요**
`RoomMemberCardProvider`는 로비 씬에서 멤버 정보 표시 카드(`RoomMemberCard`)를 효율적으로 생성하고 재사용하기 위해 `ObjectPool`을 관리하는 서비스입니다.

**필드 (Fields)**
- `_pool`: `RoomMemberCard` 객체를 관리하는 객체 풀(`ObjectPool`).
- `_data`: 풀 크기 등 설정 데이터를 담은 `RoomViewData`.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 UI 설정 데이터 및 프리팹을 초기화하고, `InitPool()`을 호출하여 풀을 생성합니다.
- **`InitPool`**: `ObjectPool`을 초기화하고, `DefaultCount`만큼의 카드를 미리 생성하여 풀에 넣습니다.
- **`GetCard(Vector3 worldPos)`**: 풀에서 카드 객체를 가져와 지정된 월드 위치에 맞춰 초기 위치를 설정합니다.
- **`ReleaseCard`**: 사용이 끝난 카드 객체를 풀로 반환합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 및 타입들입니다.*
- **RoomMemberCard (Class)**: 풀링되는 UI 카드.
- **RoomViewData (ScriptableObject)**: 풀링 설정 데이터(Default/Max Count 등).
