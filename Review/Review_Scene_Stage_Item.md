# Review_Scene_Stage_Item.md

## 1. 현황 (Observation)
- `Carriable.cs`: 모든 아이템의 기반 클래스로, `AttachableBehaviour`를 상속받아 아이템 탈부착 및 네트워크 동기화(`INetworkUpdateSystem`)를 관리합니다.
- `ItemProvider.cs`: 아이템 스폰/풀링 등을 담당하는 로직이 포함된 것으로 예상되는 클래스입니다. (읽기 필요)

## 2. 리스크 (Risk)
- **객체 구조 복잡도:** `Carriable` 클래스에서 `transform.parent.GetComponentInChildren<...>`을 다수 호출하고 있습니다. 이는 프리팹 계층 구조에 대한 의존성이 매우 높음을 의미하며, 프리팹 구성 변경 시 런타임 에러 발생 위험이 큽니다.
- **물리 처리:** `NetworkUpdate`에서 물리 오브젝트의 위치를 매 프레임 강제로 동기화하는 로직(`SyncWithNetObjPosition`)은 네트워크 지연 시 `Jitter`를 발생시킬 수 있습니다.
- **네트워크 권한:** `HasAuthority` 체크가 세밀하게 들어가 있으나, 로직이 비동기 이벤트(`OnAttachStateChanged`)에 의존하여 상태 동기화가 클라이언트 환경에 따라 어긋날 수 있습니다.

## 3. 제안 (Proposal)
- **의존성 주입 강화:** `Carriable`의 `ConstructBase`에서 컴포넌트를 `GetComponentInChildren`으로 찾지 말고, 에디터상에서 `[SF]`를 통해 명시적으로 할당하거나, `VContainer`가 인스턴스화할 때 전달하도록 하십시오.
- **물리 동기화 개선:** 매 프레임 위치를 강제 동기화하는 대신 `NetworkTransform` 컴포넌트의 설정을 최적화하여 네트워크 엔진 차원에서 처리되도록 개선하십시오.
- **계층 구조 독립성:** `IDespawnable` 등을 직접 구현하여, `transform.parent`와 같은 깊은 계층 참조를 피하십시오.
