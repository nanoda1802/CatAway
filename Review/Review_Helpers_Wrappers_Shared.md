# Review_Helpers_Wrappers_Shared.md

## 1. 현황 (Observation)
- `TweenHandler.cs`: `PrimeTween` 라이브러리를 사용하여 UI 애니메이션과 효과를 추상화한 클래스입니다.
- `VfxHandler.cs`: 파티클 시스템 제어(`Play`, `Stop`)를 래핑한 유틸리티 클래스입니다.
- `AttachableSlot.cs`: `AttachableNode`를 상속받아 아이템 탈부착 이벤트를 노출하는 래퍼 클래스입니다.
- `Thumbnail.cs`: 컴포넌트 캐싱(`GetComponent`)을 자동화하는 래퍼 컴포넌트입니다.
- `SceneChanger.cs`: 로컬 및 네트워크(`Netcode`) 환경에서의 씬 로드 흐름을 추상화하여 일관된 씬 로드 인터페이스를 제공합니다.

## 2. 리스크 (Risk)
- **성능/유지보수:** 
    - `Thumbnail.cs`의 `OnEnable`에서 호출되는 `GetComponent`는 성능에 큰 영향은 없으나, 컴포넌트 구조 변경에 취약합니다.
    - `TweenHandler`는 `Action`을 사용하여 완료 콜백을 받는데, `Sequence` 객체가 제대로 해제되지 않거나 관리되지 않을 경우 메모리 누수의 위험이 있습니다.
- **아키텍처/유지보수:**
    - `SceneChanger`가 `NetworkManager`가 null인지 체크(`InNetwork`)하는 로직이 런타임에 수행되는데, `Session` 관리 관점에서 의존성을 더 명확히 주입받아 관리하는 것이 좋습니다.
    - `AttachableSlot`의 `OnAttach/Detach` 이벤트는 `OnNetworkPreDespawn`에서 null로 초기화해주지만, 다른 이벤트나 프로퍼티들의 해제 여부를 점검할 필요가 있습니다.
- **코드 일관성:** `VfxHandler` 등 일부 헬퍼들이 인스턴스화하여 사용되는지, 싱글톤인지 명확하지 않습니다. 프로젝트 전체적으로 유틸리티 클래스의 접근 전략(싱글톤 vs DI 주입)을 통일할 필요가 있습니다.

## 3. 제안 (Proposal)
- **DI 활용 강화:** 모든 헬퍼(`TweenHandler`, `VfxHandler`)를 VContainer에 등록하고 인스턴스화하여 관리함으로써, 서비스 수준에서의 생명주기 관리를 일관되게 수행하십시오.
- **메모리 안전성 (Cleanup):**
    - `TweenHandler`의 모든 콜백은 `DisposableBag` 등에 등록하여 씬 종료 시 안전하게 중단되도록 보장하십시오.
    - `SceneChanger`의 `SceneEvent` 구독 해제 로직을 더 견고하게 만드십시오.
- **테스트 코드 도입:** `SceneChanger`의 씬 로드 요청 처리를 순수 C# 로직으로 분리하여 테스트하십시오. 특히 네트워크/로컬 로드 분기 로직이 정상 작동하는지 검증하는 테스트가 필요합니다.
- **일관성 제고:** 헬퍼 클래스들의 명명 규칙 및 접근 권한(싱글톤 vs DI)을 통일하십시오.
