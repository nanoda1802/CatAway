# Plate.cs 기술 문서

**개요**
`Plate`는 요리 완성품을 담는 접시 클래스입니다. `IIngredientHolder`를 구현하여 완성된 식재료를 담고, 플레이팅(`Plating`) 상태를 제어합니다.

**필드 (Fields)**
- `Plating`: 현재 담겨 있는 요리 재료.
- `_sharedIsWellPrepped`: 조리 완료 상태 동기화 변수.

**주요 메서드 (Methods)**
- **`Hold(Ingredient ingredient)`**: 요리된 식재료를 접시에 담습니다.
- **`ClearHolder(bool isComp)`**: 담긴 재료를 비웁니다.
