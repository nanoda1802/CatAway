`C:\Users\home\Documents\GitHub\PartTimeCat\Assets\_Scripts\Stage\Table`

# Stage 모듈: Table 관련 컴포넌트

이 문서는 `Assets/_Scripts/Stage/Table/` 하위의 테이블 시스템, 상호작용 로직, 인터페이스 및 구현체들에 대해 설명합니다.

## 1. 개요 (Overview)
스테이지 내 상호작용 가능한 모든 가구(Table)들을 관리하는 시스템입니다. 플레이어의 동작(물건 놓기, 상호작용, 접촉 등)이 테이블과 어떻게 결합되는지를 정의하며, 브로커 패턴을 통해 복잡한 상호작용 로직을 분리합니다.

## 2. 중개자 로직 (Broker System)
테이블 상호작용의 핵심 로직을 담당하는 브로커 컴포넌트입니다.

*   **PlacementBroker.cs**: 플레이어가 물건을 들고 테이블과 상호작용할 때, `HandlePlaceCase`(물건 놓기), `HandlePickCase`(물건 집기), `HandleHoldCase`(재료 부착) 등의 복잡한 케이스를 판단하고 실행합니다.
*   **ContactBroker.cs**: 플레이어나 아이템이 테이블에 직접 접촉했을 때 발생하는 상호작용(예: 쓰레기통, 서빙 등)을 단발성(`RespondTo`)으로 처리합니다.

## 3. 핵심 인터페이스 (Table Interfaces)

각 테이블은 다음 인터페이스를 구현하여 자신의 기능을 정의합니다.

*   **IPlacable**: 물건을 올려두거나(`Place`) 가져갈 수 있는(`Displace`) 테이블입니다. 각 동작 전 `CanPlace`와 `CanDisPlace` 메서드를 통해 아이템 부착 및 분리 조건을 검사합니다.
    *   구현체: `BoxTable`, `ChoppingTable`, `PantryTable`, `PlateRackTable`, `PlateReturnTable`, `StoveTable`.
*   **IInteractable**: 플레이어가 버튼을 길게 누르는(`Hold` 기반) 상호작용을 처리하는 테이블입니다. `TryInteraction`을 통해 상호작용을 시작하고, `CancelInteraction`을 통해 중지하며, `FinishInteraction`을 통해 완료합니다.
    *   구현체: `SinkTable`, `ChoppingTable`.
*   **IContactable**: `IInteractable`과 대조되는 단발성(`Press` 기반) 접촉 상호작용 테이블입니다. 접촉 시 `TryContact`로 조건을 검사하고 즉각적으로 `RespondTo`를 수행합니다.
    *   구현체: `BinTable`, `ServingTable`, `SinkTable`.

## 4. 테이블 구현체 상세

### 4.1 Contactable (Press 기반)
*   **BinTable**: 던져진 아이템을 받거나 접촉 시 내용을 비우는 테이블.
*   **ServingTable**: 완성된 요리를 제출하는 테이블.
*   **SinkTable**: 그릇을 세척하는 테이블.

### 4.2 Placable 테이블 (Hold 기반)
*   **BoxTable**: 아이템을 보관하는 박스 테이블.
*   **ChoppingTable**: 식재료를 썰어 조리하는 테이블.
*   **PantryTable**: 식재료를 생성 및 제공하는 테이블.
*   **PlateRackTable**: 깨끗한 접시를 관리하는 랙.
*   **PlateReturnTable**: 사용한 접시를 반납받는 테이블.
*   **StoveTable**: 화력으로 조리하는 테이블.

## 5. 기타 컴포넌트
*   **TablePrefabHandler.cs**: `IObjectResolver`를 통해 테이블 프리팹 인스턴스화 시 의존성 주입을 수행하는 관리자 컴포넌트.

---
## Feedback
- **분석 결과**: `PlacementBroker`와 `ContactBroker`를 통해 복잡한 테이블 상호작용을 중앙화하여 관리하고 있습니다. 인터페이스(`IPlacable`, `IInteractable`, `IContactable`)를 활용한 다형성 구현이 매우 뛰어나며, VContainer를 통한 의존성 주입이 깔끔하게 적용되어 있습니다.
- **정합성**: 코드 내 브로커 시스템과 인터페이스 구조가 문서와 완벽히 일치하며, 메시지 브로커를 통한 상호작용 체계가 견고하게 유지되고 있습니다.

