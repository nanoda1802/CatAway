# StageHub.cs 기술 문서

**개요**
`StageHub`는 스테이지 씬 내의 주요 서비스(테이블, 제공자, UI 프레젠터 등)를 등록하고 관리하는 중앙 허브 서비스입니다. 다른 시스템들이 이 허브를 통해 필요한 서비스에 접근할 수 있습니다.

**필드 (Fields)**
- `_placableDic`, `_providerDic`: 테이블 및 아이템 제공자들을 타입별로 관리하는 딕셔너리.
- `_plateReturnTableDic`, `_scorePresenterDic`, `_orderPresenterDic`: 팀별로 관리되는 테이블 및 UI 프레젠터 딕셔너리.
- `_cuePresenter`: 스테이지 큐를 처리하는 프레젠터.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 각종 인터페이스(IPlacable, IProvider 등)를 구독하고 초기화 시점(`Initialize`)에 서비스 목록을 요청(`HubCallMessage`)합니다.
- **`Fetch...` 메서드들**: 제네릭 또는 팀 단위로 테이블, 제공자, 프레젠터를 조회하여 제공합니다.
- **`Initialize`**: 허브 서비스들에 대한 접근을 요청하는 메시지를 발행합니다.
- **`Dispose`**: 서비스 관리를 위한 딕셔너리들을 정리합니다.

**참조된 타입 요약**
*본 문서화에서 제외된 객체 및 타입들입니다.*
- **IPlacable, IProvider (Interface)**: 테이블 및 아이템 제공 인터페이스.
- **ScorePresenter, OrderPresenter, CuePresenter (Class)**: 씬 내 UI 프레젠터.
- **HubCallMessage (Struct/Class)**: 서비스 조회 요청 메시지.
