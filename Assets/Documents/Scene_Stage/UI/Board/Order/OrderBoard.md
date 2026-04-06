# OrderBoard.cs 기술 문서

**개요**
`OrderBoard`는 스테이지 상단 등에 위치하여 현재 활성화된 주문 목록(`OrderCard`)을 시각적으로 배치하고 관리하는 UI 컨테이너입니다.

**주요 메서드 (Methods)**
- **`Show` / `Hide`**: 보드 UI의 활성화/비활성화 처리.
- **`UpdateOrders`**: 현재 활성 주문 목록을 기반으로 자식 카드들을 갱신합니다.
