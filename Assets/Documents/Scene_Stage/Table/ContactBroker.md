# ContactBroker.cs 기술 문서

**개요**
`ContactBroker`는 아이템 운반자가 테이블과 상호작용할 때(놓기/줍기 제외), 해당 테이블의 `IContactable` 로직을 실행하는 서비스입니다.

**주요 메서드 (Methods)**
- **`AcceptCase(CarrierBehaviour carrier, IContactable table)`**: 운반자가 들고 있는 아이템 타입에 따라 테이블의 `RespondTo` 메서드를 호출하여 상호작용을 처리합니다.
- **`AcceptCase(Ingredient ingredient, IContactable table)`**: 식재료와 테이블 간의 접촉을 처리합니다.
