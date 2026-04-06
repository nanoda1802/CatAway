# SinkTable.cs 기술 문서

**개요**
`SinkTable`은 더러워진 접시를 씻어 깨끗한 접시로 만드는 세척 테이블입니다.

**필드 (Fields)**
- `bubbleVfx`: 세척 중 재생되는 이펙트.
- `_sharedProgress`: 네트워크 동기화되는 세척 진행도.
- `_interactorList`: 현재 세척 중인 플레이어 목록.

**주요 메서드 (Methods)**
- **`NetworkUpdate`**: 플레이어의 수에 따라 세척 진행도를 가속하고 완료 시 처리합니다.
- **`TryInteraction`**: 접시가 있을 때 세척 애니메이션과 이펙트를 시작합니다.
- **`FinishInteraction`**: 세척 완료 시 접시 상태를 갱신하고 `PlateRackTable`로 이동시킵니다.
- **`ActivateProgressBarRpc` / `DeactivateProgressBarRpc`**: 세척 진행바 UI를 동기화합니다.
- **`ActivateFxRpc` / `DeactivateFxRpc`**: 세척 이펙트 및 효과음을 동기화합니다.
