# BoxTable.cs 기술 문서

**개요**
`BoxTable`은 아이템을 올려두거나 보관하는 기본 박스 테이블입니다.

**필드 (Fields)**
- `spawnWithPlate`: 스폰 시 접시 생성 여부.
- `PlacedItem`: 현재 박스에 놓인 아이템.

**주요 메서드 (Methods)**
- **`Place` / `CanPlace` / `CanDisPlace`**: 아이템 배치 및 회수 가능 여부를 판정하고 배치합니다.
- **`AttachWithSpawn`**: 설정에 따라 초기 접시를 생성하고 배치합니다.
