# SectionBase.cs 기술 문서

**개요**
`SectionBase`는 룸 씬의 다양한 UI 섹션들이 상속받는 추상 기반 클래스입니다. UI의 표시(`Show`) 및 숨김(`Hide`) 인터페이스를 강제하고, 공통적인 리소스 해제(`DisposableBagBuilder`) 및 비동기 작업 관리 기능을 제공합니다.

**필드 (Fields)**
- `DisposableBagBuilder`: 섹션에서 사용하는 메시지 구독 등 비동기/IDisposable 자원을 관리하는 도구.

**주요 메서드 (Methods)**
- **`Show` / `Hide` (Abstract)**: 각 섹션에서 구현해야 할 비동기 UI 표시/숨김 로직입니다.
- **`RefreshToken`**: 비동기 작업을 위한 새로운 `CancellationToken`을 생성하여 이전 작업을 취소하고 관리합니다.
- **`OnDestroy`**: 섹션 파괴 시 자원을 해제하고 비동기 작업(`DisposableBagBuilder`)을 정리합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 타입들입니다.*
- **UniTask (Library)**: 비동기 작업 관리.
