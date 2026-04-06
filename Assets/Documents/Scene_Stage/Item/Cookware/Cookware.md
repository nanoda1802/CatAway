# Cookware.cs 기술 문서

**개요**
`Cookware`는 식재료를 담아 조리할 수 있는 도구 클래스입니다. `IIngredientHolder` 인터페이스를 구현하여 식재료를 담고, 가열(`Prepare`) 및 플레이팅 상태를 관리합니다.

**필드 (Fields)**
- `HeldIngredient`: 현재 담겨 있는 식재료.
- `_sharedHasIngredient`: 재료 보유 여부 동기화 변수.

**주요 메서드 (Methods)**
- **`Hold(Ingredient ingredient)`**: 식재료를 도구에 담습니다.
- **`Prepare(float progress)`**: 가열 진행도를 갱신합니다.
- **`ClearHolder(bool isComp)`**: 담긴 재료를 비웁니다.
