# BinTable.cs 기술 문서

**개요**
`BinTable`은 접촉한 아이템을 폐기(Despawn) 처리하는 쓰레기통 테이블입니다.

**필드 (Fields)**
- `_stageHub`, `_contactBroker`: 테이블 기능 수행을 위한 서비스들.
- `_itemTag`: 아이템 식별용 태그.

**주요 메서드 (Methods)**
- **`Construct`**: 의존성 주입을 통해 서비스와 태그를 초기화합니다.
- **`OnTriggerEnter`**: 투척된 식재료와 접촉 시 `ContactBroker`를 통해 폐기 로직을 실행합니다.
- **`TryContact`**: 아이템이 식재료이거나 내용물이 있는 홀더일 경우만 접촉을 허용합니다.
- **`RespondTo`**: 아이템을 폐기하거나(Despawn), 홀더 내부를 비우는 로직을 수행합니다.
