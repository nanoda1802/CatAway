# PantryTable_BackUp.cs 기술 문서

**개요**
`PantryTable_BackUp`은 특정 식재료를 지속적으로 공급하는 팬트리 테이블입니다.

**필드 (Fields)**
- `presetType`: 공급할 식재료 타입(`IngredientType`).
- `sampleMeshFilter`, `sampleTransform`: 식재료의 시각적 모델 정보.
- `_sharedIngredientType`: 네트워크 동기화되는 식재료 타입.

**주요 메서드 (Methods)**
- **`Construct`**: `StageHub`를 의존성 주입받습니다.
- **`OnNetworkSpawn` / `OnNetworkPostSpawn`**: 서버에서 타입을 동기화하고, 모델을 업데이트합니다.
- **`UpdateSampleModel`**: 제공자(`IngredientProvider`)로부터 모델 데이터를 가져와 메쉬를 갱신합니다.
- **`RespondTo`**: 플레이어가 접촉하면 공급 가능한 재료를 생성하여 플레이어에게 부착(`Pick`)합니다.
