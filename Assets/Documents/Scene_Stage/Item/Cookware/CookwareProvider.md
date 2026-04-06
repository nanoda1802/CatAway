# CookwareProvider.cs 기술 문서

**개요**
`CookwareProvider`는 조리기구(`Cookware`)의 `ObjectPool`을 관리하는 서비스입니다.

**주요 메서드 (Methods)**
- **`InitPool`**: 조리기구 풀을 생성하고 초기화합니다.
- **`GetCookware(Vector3 pos)`**: 풀에서 조리기구를 가져와 위치를 설정하고 반환합니다.
- **`Release(Cookware item)`**: 사용 완료된 조리기구를 풀로 반환합니다.
