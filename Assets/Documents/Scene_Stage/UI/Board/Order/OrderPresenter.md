# OrderPresenter.cs 기술 문서

**개요**
`OrderPresenter`는 주문 시스템의 로직을 관리하며 `OrderBoard` UI와 연동하여 현재 주문 상태를 갱신합니다.

**주요 메서드 (Methods)**
- **`CheckRecipe`**: 완성된 접시의 요리 데이터가 주문 목록과 일치하는지 확인하고 점수를 계산합니다.
- **`AddOrder` / `RemoveOrder`**: 주문 목록에 새로운 주문을 추가하거나 완료된 주문을 제거하고 보드를 갱신합니다.
