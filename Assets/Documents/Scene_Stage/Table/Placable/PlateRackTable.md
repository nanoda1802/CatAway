# PlateRackTable.cs 기술 문서

**개요**
`PlateRackTable`은 씻은 접시들을 쌓아서 보관하는 랙입니다.

**필드 (Fields)**
- `plateOffsetY`: 접시를 쌓을 때 필요한 높이 간격.
- `_washedPlates`: 보관 중인 접시 스택.

**주요 메서드 (Methods)**
- **`OnSlotAttached` / `OnSlotDetached`**: 접시가 쌓이거나 회수될 때의 위치를 업데이트합니다.
- **`Place` / `CanPlace`**: 깨끗한 접시만 보관 가능하도록 배치 로직을 처리합니다.
