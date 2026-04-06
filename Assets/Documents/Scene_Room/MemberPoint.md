# MemberPoint.cs 기술 문서

**개요**
`MemberPoint`는 로비 씬 내에서 `RoomMember`가 위치하는 지점을 나타내는 컴포넌트입니다. 멤버의 배치, 할당 및 위치 이동(교체) 로직을 관리합니다.

**필드 (Fields)**
- `PointIdx`: 해당 지점의 인덱스 식별자.
- `_curMem`: 현재 이 지점에 할당된 `RoomMember`.
- `Pos`, `Rot`: 위치와 회전값.

**주요 메서드 (Methods)**
- **`Init`**: 지점의 인덱스와 멤버 교체 이벤트(`OnSwap`)를 초기화합니다.
- **`Assign` / `Resign`**: `RoomMember`를 해당 지점에 할당하거나 해제합니다.
- **`SwapMem`**: 다른 `MemberPoint`와 현재 멤버를 교체하거나 위치를 이동시킵니다. 이동 완료 후 `OnSwap` 이벤트를 발생시켜 상태 변경을 알립니다.

**참조된 타입 요약**
*본 문서화에서 제외된 객체입니다.*
- **RoomMember (Class)**: 배치 대상인 로비 멤버.
