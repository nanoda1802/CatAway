# PantryTable.cs 기술 문서

**개요**
`PantryTable`은 특정 식재료를 무한 공급하는 테이블입니다.

**필드 (Fields)**
- `presetType`: 공급할 식재료 타입(`IngredientType`).
- `_sharedIngredientType`: 네트워크 동기화 식재료 타입.

**주요 메서드 (Methods)**
- **`AttachWithSpawn`**: 식재료를 생성하여 테이블에 배치합니다.
- **`Place`**: 재료 회수 시 다시 재료를 생성하도록 로직이 구현되어 있습니다.
- **`CanPlace`**: 아이템을 둘 수 없는 테이블입니다.
