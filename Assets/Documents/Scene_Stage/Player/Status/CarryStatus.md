# CarryStatus.cs 기술 문서

**개요**
`CarryStatus`는 플레이어가 아이템을 운반하는 상태와 관련된 쿨타임 및 운반 중인 아이템 정보를 관리하는 데이터 클래스입니다.

**필드 (Fields)**
- `CurCarriable`: 현재 운반 중인 아이템(`Carriable`).
- `_lastCarryTime`: 마지막 운반 상호작용 시각.

**주요 메서드 (Methods)**
- **`IsCarryAvailable`**: 운반 인터벌(`CarryInterval`)을 기반으로 재운반 가능 여부를 반환합니다.
- **`UpdateLastCarryTime`**: 마지막 운반 시간을 현재 시각으로 갱신합니다.
