## Package In Project

### Network

- **Netcode for GameObjects :** 멀티플레이 구현용, 추후 Relay 등 유니티의 부가적인 네트워크 패키지와의 호환성 고려
- **Multiplayer Samples Utilities :** ClientNetworkTransform, ClientNetworkAnimator 등, 클라이언트 주도 동기화에 필요한 Component 활용 위해 설치 (네트워크 지연에 의한 플레이 불편 줄이기 위함)
- **Multiplayer Play Mode :** 개발 과정에서의 네트워크 기능 테스트 위해 설치, Project Build 또는 PararellSync 등 다른 테스트 방식과 달리, 프로젝트 클론 방식이 아니라서 별도의 용량 차지하지 않는다는 점, 각 가상 플레이어에서 별도의 Hierarchy, Inspectorm, Console 창 등을 확인 가능

### Core Dependencies

- **VContainer :** GetComponent, Find 등 MonoBehaviour 의존적인 참조 방식 극복, 오브젝트 간 의존성의 가시성 향상, 디버깅 경로 일원화 등등의 이유로 설치
- **MessagePipe :** 스크립트 간 결합도 낮추기, Publisher와 Subscriber 기능 활용한 메세지 기반 반응형 프로그래밍 의도 (특히 View와 Logic 간)
- **UniTask :** Coroutine 및 async/await 대체. 구조체 기반 작동 방식에 의거해 GC call 최소화, 메모리 효율성 향상 의도
- **Prime Tween :** View 오브젝트들의 생동감 향상 연출, 구조체 기반 방식으로 다른 Tween 라이브러리들에 비해 성능적 우위

### Unity Registry

- **Input System :** PC와 Mobile 전환 간 입력 방식의 효율적인 스위칭 의도
- **2D Tilemap Editor :** Stage의 Level 생성 작업 시 능률 향상 의도
- **2D Tilemap Extras :** 3D 오브젝트의 Tilemap 패키지 호환 위해 설치 (Tilemap Pallete의 GameObject Brush 활용)
- **FBX Exporter :** Blender에서의 간단한 Mesh 병합 및 Mesh 단순화 목적으로 fbx 모델 추출하기 위해 설치