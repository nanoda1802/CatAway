# DetectStatus.cs 기술 문서

**개요**
`DetectStatus`는 플레이어 전방의 아이템이나 테이블을 감지하기 위한 로직을 수행하는 서비스입니다.

**필드 (Fields)**
- `_detectPoint`: 탐지 기준이 되는 `Transform`.
- `_data`: 탐지 범위를 정의하는 `PlayerData`.
- `_detectedItems`: 아이템 감지를 위한 캐싱용 배열.

**주요 메서드 (Methods)**
- **`DetectItem`**: `OverlapBox`를 사용하여 주변의 아이템(`Carriable`) 중 가장 가까운 객체를 탐지하여 반환합니다.
- **`DetectTable`**: `Raycast`를 사용하여 전방의 테이블(`NetworkObject`)을 탐지합니다.
