# PlateReturnTable.cs 기술 문서

**개요**
`PlateReturnTable`은 요리된 접시를 최종적으로 회수하거나 제출하는 팀별 테이블입니다.

**필드 (Fields)**
- `plateOffsetY`: 접시를 쌓을 때 높이 간격.
- `team`: 소속 팀.
- `_returnedPlates`: 보관 중인 접시 스택.

**주요 메서드 (Methods)**
- **`OnSlotAttached` / `OnSlotDetached`**: 접시가 반납되거나 회수될 때 위치를 조정합니다.
- **`Place` / `CanPlace`**: 빈 접시만 반납 가능하도록 로직을 처리합니다.
