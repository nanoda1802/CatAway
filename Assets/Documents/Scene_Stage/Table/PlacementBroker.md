# PlacementBroker.cs 기술 문서

**개요**
`PlacementBroker`는 플레이어와 테이블 간의 아이템 배치/회수 상호작용(`IPlacable`)을 처리하는 중개 서비스입니다.

**주요 메서드 (Methods)**
- **`AcceptCase(CarrierBehaviour carrier, IPlacable table)`**: 플레이어의 운반 아이템과 테이블의 배치 아이템 상태에 따라 줍기, 배치, 합치기(Ingredient-to-Holder) 등의 로직을 처리합니다.
- **`AcceptCase(Ingredient ingredient, IPlacable table)`**: 단일 식재료와 테이블 간의 배치/합치기 로직을 처리합니다.
- **`HandlePickCase` / `HandlePlaceCase` / `HandleHoldCase` / `HandleHolderToHolderCase`**: 각 상호작용 시나리오별로 아이템의 타입과 상태를 검사하고 처리 결과를 반환합니다.
