# ChoppingTable.cs 기술 문서

**개요**
`ChoppingTable`은 식재료를 다듬어(Prepare) 요리할 수 있는 상태로 만드는 테이블입니다.

**필드 (Fields)**
- `chopVfx`: 다듬기 중 이펙트.
- `ingredientMask`: 다듬기 가능한 식재료 타입 정의.
- `_sharedProgress`: 네트워크 동기화되는 진행도.
- `_targetIngredient`: 현재 다듬고 있는 식재료.

**주요 메서드 (Methods)**
- **`NetworkUpdate`**: 조리 진행도를 업데이트하고 완료 시 처리합니다.
- **`TryInteraction`**: 다듬기 애니메이션과 이펙트를 시작합니다.
- **`FinishInteraction`**: 식재료의 다듬기 완료 상태를 갱신합니다.
- **`Place` / `CanPlace`**: 식재료 배치 가능 여부를 판정하고 처리합니다.
