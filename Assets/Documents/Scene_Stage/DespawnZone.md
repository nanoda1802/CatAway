# DespawnZone.cs 기술 문서

**개요**
`DespawnZone`은 게임 스테이지에서 특정 객체(아이템, 플레이어)가 영역을 벗어났을 때 자동으로 디스폰 처리를 수행하는 트리거 영역 컴포넌트입니다.

**필드 (Fields)**
- `_itemTag`, `_playerTag`: 대상 객체를 식별하기 위한 태그 핸들러.

**주요 메서드 (Methods)**
- **`Awake`**: 아이템 및 플레이어 태그를 캐싱합니다.
- **`OnTriggerEnter(Collider other)`**: 트리거 영역에 진입한 객체의 태그를 검사합니다. 아이템이나 플레이어인 경우 부모 객체로부터 `IDespawnable` 인터페이스를 찾아 `Despawn()`을 호출합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 타입들입니다.*
- **IDespawnable (Interface)**: 디스폰 기능을 제공하는 인터페이스.
- **TagHandle (Class)**: 태그 식별용 유틸리티.
