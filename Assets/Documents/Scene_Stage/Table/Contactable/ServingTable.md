# ServingTable.cs 기술 문서

**개요**
`ServingTable`은 요리가 완성된 접시를 제출하여 주문을 완료하는 서빙 테이블입니다.

**필드 (Fields)**
- `team`: 이 테이블이 속한 팀(`Team`).
- `_stageHub`: 스테이지 씬 서비스 접근 허브.

**주요 메서드 (Methods)**
- **`TryContact`**: 완성된 접시인지 검사합니다.
- **`RespondTo`**: 주문과 일치하는지 확인하고, 점수를 계산하여 `ToastWidget`을 표시합니다. 일치 시 접시를 회수하고 `PlateReturnTable`로 전달합니다.
- **`ActivateToastWidgetRpc`**: 점수 알림 위젯을 활성화합니다.
