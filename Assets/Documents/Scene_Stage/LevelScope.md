# LevelScope.cs 기술 문서

**개요**
`LevelScope`는 스테이지 씬의 의존성 관리 범위를 정의하는 `LifetimeScope`입니다. 스테이지 초기화 서비스와 핵심 게임 데이터를 컨테이너에 등록합니다.

**필드 (Fields)**
- `ingredientDataList`: 재료별 데이터를 담은 `IngredientData` 배열.
- `plateData`: 접시 관련 데이터를 담은 `PlateData`.

**주요 메서드 (Methods)**
- **`Configure(IContainerBuilder builder)`**: DI 컨테이너를 설정합니다.
    - **`RegisterEntryPoint<LevelInitiator>`**: 스테이지 초기화 서비스를 엔트리 포인트로 등록합니다.
    - **`RegisterInstance`**: 재료 데이터 배열과 접시 데이터를 싱글톤으로 제공합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 데이터 및 타입들입니다.*
- **IngredientData, PlateData (ScriptableObject)**: 게임 내 스테이지 아이템 데이터.
- **LevelInitiator (Class)**: 스테이지 초기화 서비스.
