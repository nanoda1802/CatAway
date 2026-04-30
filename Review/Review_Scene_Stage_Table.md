# Review_Scene_Stage_Table.md

## 1. 현황 (Observation)
- `PlacementBroker.cs`: 플레이어의 아이템 상호작용(`CarrierBehaviour`)과 테이블(`IPlacable`) 사이의 상태 전환 로직을 `switch`문과 `Pattern Matching`을 사용하여 처리합니다.

## 2. 리스크 (Risk)
- **복잡도와 확장성:** `switch`문에 `(null, Ingredient)` 조합 등을 명시하여 모든 경우의 수를 처리하고 있습니다. 아이템 종류(Ingredient, Plate, Cookware 등)나 테이블 종류가 늘어날 때마다 `switch`문의 조합이 지수적으로 증가하여 유지보수가 매우 어려워질 것입니다.
- **안전성:** `BrokerResult`를 반환하여 에러 처리를 하고 있으나, `(CarrierBehaviour, IPlacable)` 조합에서 잘못된 타입 캐스팅 시 런타임 예외 발생 가능성이 존재합니다.
- **결합도:** 상호작용 가능한 아이템들의 인터페이스(`IIngredientHolder` 등)에 의존하고 있어, 새로운 아이템 타입 추가 시 인터페이스의 변화가 불가피합니다.

## 3. 제안 (Proposal)
- **전략 패턴/상태 머신 적용:** `PlacementBroker`를 거대한 `switch`문으로 두지 말고, `IInteractionRule`과 같은 규칙 인터페이스를 정의하고 리스트로 관리하십시오. 각 아이템 조합별로 `Rule` 클래스를 만들어 처리하면 확장성이 비약적으로 높아집니다.
- **타입 안정성 강화:** `is` 패턴 매칭을 활용하되, 캐스팅 성공 여부를 명확히 분리하여 런타임 예외를 방지하는 구조로 개선하십시오.
- **단위 테스트:** 현재 로직이 `PlacementBroker` 하나에 집중되어 있으므로, 다양한 아이템/테이블 조합에 대한 단위 테스트(`TableInteractionTests`)를 작성하십시오.
