`C:\Users\home\Documents\GitHub\PartTimeCat\Assets\_Scripts\Stage\Item`

# Stage 모듈: Item 관련 컴포넌트

이 문서는 `Assets/_Scripts/Stage/Item/` 하위의 아이템 관련 로직, 운반, 조리 기구, 식재료, 접시 등 아이템 시스템 전반에 대해 설명합니다.

## 1. 개요 (Overview)
게임 내에서 플레이어가 상호작용하는 모든 물건(조리 기구, 식재료, 접시)의 기반이 되는 시스템입니다. `Carriable`을 기반으로 부착 가능한 물체로 정의되며, `NetworkObjectPool`을 활용한 효율적인 객체 관리(`ItemProvider`)를 수행합니다.

## 2. 핵심 기반 클래스

### 2.1 `Carriable.cs`
모든 운반 가능한 아이템의 부모 클래스입니다. `AttachableBehaviour`를 상속받아 아이템의 부착/탈착 상태를 네트워크로 동기화합니다.
*   **주요 기능:** 운반 상태 관리, 물리 컴포넌트(Rigidbody, Collider) 제어.

### 2.2 `ItemProvider<T>.cs`
`NetworkObjectPool`을 관리하는 제네릭 클래스입니다. 아이템 생성, 회수, 풀링을 담당하여 메모리를 최적화합니다.
*   **주요 기능:** 오브젝트 풀 관리, 아이템 생성/파괴 요청 처리.

## 3. 주요 아이템 컴포넌트

### 3.1 `Cookware` (조리 기구)
`IIngredientHolder`를 구현하여 식재료를 담고 조리할 수 있는 상태를 관리합니다.
*   **주요 기능:** 특정 타입의 식재료 수용 가능 여부 판별, 조리 기구 내부의 식재료 부착 처리.

### 3.2 `Ingredient` (식재료)
`IPrepable`을 구현하여 조리 진행률에 따라 상태가 변하는 식재료입니다.
*   **주요 기능:** 조리 상태(Raw, WellDone)에 따른 모델 교체, 던지기 물리 연산 및 넉백 처리.

### 3.3 `Plate` (접시)
`IPrepable`, `IIngredientHolder`를 모두 구현하여 식재료를 담아 요리를 완성하는 최종 그릇입니다.
*   **주요 기능:** Plating(식재료 결합) 마스크 관리, 요리 완성 상태 시각화, `PlatingIconWidget`을 통한 UI 연동.

## 4. 인터페이스 정의
*   `IIngredientHolder`: 식재료를 담을 수 있는 객체(Cookware, Plate)의 표준 인터페이스.
*   `IPrepable`: 조리가 가능한 객체(Ingredient, Plate)의 표준 인터페이스.

## 5. Prefab Handling
각 아이템 클래스별로 `...PrefabHandler`가 존재하며, 이는 `NetworkManager.PrefabHandler`에 등록되어 클라이언트가 서버로부터 아이템 생성을 요청받을 때 적절한 아이템 인스턴스를 풀에서 가져오거나 생성하도록 합니다.


---
## Feedback
- 분석 결과: `ItemProvider<T>`가 `NetworkObjectPool`과 연동되어 효율적인 아이템 생명주기 관리를 수행하고 있습니다. `Carriable`을 통한 부착 상태 관리와 `MessagePipe`를 통한 상호작용 이벤트 처리가 일관성 있게 구현되어 있습니다.
- 정합성: 코드와 문서의 아이템 생성 및 풀링 로직이 일치하며, 의존성 주입이 각 프리팹 핸들러를 통해 올바르게 처리되고 있습니다.

