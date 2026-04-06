# PlateProvider.cs 기술 문서

**개요**
`PlateProvider`는 접시(`Plate`)의 `ObjectPool`을 관리하는 서비스입니다.

**주요 메서드 (Methods)**
- **`GetPlate(Vector3 pos)`**: 풀에서 접시를 가져와 반환합니다.
- **`Release(Plate item)`**: 사용 완료된 접시를 풀로 반환합니다.
- **`HasInactivePlate`**: 현재 풀 내 사용 가능한 접시가 있는지 확인합니다.
