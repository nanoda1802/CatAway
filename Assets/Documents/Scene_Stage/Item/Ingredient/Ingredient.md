# Ingredient.cs 기술 문서

**개요**
`Ingredient`는 조리/다듬기가 가능한 식재료 클래스입니다. `IPrepable` 인터페이스를 구현하며, `NetworkVariable`로 상태(Raw, WellPrepped, MaxPrepped)를 동기화하고 조리 진행도(Progress)를 관리합니다.

**필드 (Fields)**
- `Type`: 식재료 타입(`IngredientType`).
- `_sharedStatus`: 상태 정보 동기화 변수.

**주요 메서드 (Methods)**
- **`Prepare(float progress)`**: 조리/다듬기 진행도를 갱신합니다.
- **`OnPrepCompleted`**: 조리/다듬기 완료 시 상태를 변경합니다.
- **`Throw(Vector3 pos, Quaternion rot, Vector3 dir)`**: 아이템을 던지는 비동기 로직을 수행합니다.
