# InteractStatus.cs 기술 문서

**개요**
`InteractStatus`는 플레이어가 상호작용 중인지 여부와 마지막 상호작용 쿨타임을 관리하는 서비스입니다.

**필드 (Fields)**
- `CurInteractable`: 현재 상호작용 중인 대상 인터페이스(`IInteractable`).
- `_lastInteractTime`: 마지막 상호작용 시각.

**주요 메서드 (Methods)**
- **`IsInteractAvailable`**: 상호작용 인터벌(`InteractInterval`)을 기반으로 상호작용 가능 여부를 반환합니다.
- **`StartInteractionAnim` / `StopInteractionAnim`**: 상호작용 애니메이션을 관리하기 위해 파라미터 해시를 기반으로 애니메이터의 bool 값을 설정합니다.
