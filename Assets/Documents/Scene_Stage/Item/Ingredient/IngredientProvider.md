# IngredientProvider.cs 기술 문서

**개요**
`IngredientProvider`는 식재료(`Ingredient`)의 `ObjectPool`을 관리하는 서비스입니다.

**주요 메서드 (Methods)**
- **`GetIngredient(IngredientType type, Vector3 pos)`**: 특정 타입의 식재료를 생성하여 반환합니다.
- **`GetModelInfo(IngredientType type)`**: 식재료 타입별 시각적 메쉬 및 스케일 데이터를 반환합니다.
- **`Release(Ingredient item)`**: 사용 완료된 식재료를 풀로 반환합니다.
