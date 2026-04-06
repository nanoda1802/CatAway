# Carriable.cs 기술 문서

**개요**
`Carriable`은 운반 가능한 모든 객체의 기본 클래스입니다. `AttachableBehaviour`를 상속받아 아이템이 플레이어/테이블에 부착/분리될 때의 위치(LocalPosition/Rotation)와 네트워크 동기화 상태(`NetworkVariable<bool>`)를 관리합니다.

**필드 (Fields)**
- `_sharedIsCarrying`: 현재 운반 중인지 여부를 동기화하는 `NetworkVariable`.

**주요 메서드 (Methods)**
- **`Attach(AttachableBehaviour parent)`**: 객체를 다른 오브젝트에 부착합니다.
- **`Detach`**: 객체를 부착된 부모로부터 분리합니다.
