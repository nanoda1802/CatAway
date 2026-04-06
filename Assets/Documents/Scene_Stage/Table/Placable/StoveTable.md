# StoveTable.cs 기술 문서

**개요**
`StoveTable`은 가열하여 식재료를 조리하는 스토브 테이블입니다.

**필드 (Fields)**
- `fireVfx`: 가열 중 이펙트.
- `_placedCookware`: 올려진 조리기구.
- `_sharedProgress`: 네트워크 동기화 진행도.

**주요 메서드 (Methods)**
- **`NetworkUpdate`**: 가열 진행도를 갱신하고 완료 시 처리합니다.
- **`StartHeat` / `FinishHeat`**: 가열을 시작하고 완료하며, 완료 시 자동으로 과열 경고 시스템(`WarnOverHeat`)을 구동합니다.
- **`WarnOverHeat`**: 조리 후 시간 내에 회수하지 않으면 아이템이 타버리게 합니다.
- **`Place`**: 조리기구 배치 가능 여부를 판정합니다.
