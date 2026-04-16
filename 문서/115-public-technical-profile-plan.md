# 공개 기술 문서 프로파일 기획

이 문서는 공개 레포에 추가할 `기술적 측면 및 로직 상세` 문서군의 설계안이다.

목표는 두 가지다.

- 코드를 직접 읽지 않아도 KoTalk가 실제로 어떻게 동작하는지 이해되게 만들기
- 과장 없이 현재 구현된 구조와 흐름만을 근거로 공개 표면을 강화하기

## 기본 원칙

- 공개 기술 문서는 `아키텍처 개요 1개 + 실제 흐름 문서 4개` 구성이 가장 읽기 쉽다.
- 문서마다 독자를 분리한다. 하나의 문서에 구조, 인증, 메시지, UI 표면을 모두 섞지 않는다.
- “현재 구현 기준”과 “향후 계획”을 같은 문단에서 섞지 않는다.
- 소스 링크는 실제 근거를 보여 주는 최소 파일만 건다.

## 도입할 문서군

| 공개 문서 후보 | 역할 | 주 독자 | 공개 표면에서 증명할 것 |
|---|---|---|---|
| `TECHNICAL_ARCHITECTURE.md` | 시스템 구성요소와 계층 개요 | 첫 방문 기여자, 기술 평가자 | KoTalk가 제품형 계층 구조를 갖춘 저장소라는 점 |
| `AUTH_AND_BOOTSTRAP_FLOW.md` | 가입, 토큰, 세션, bootstrap 흐름 | 백엔드 기여자, 테스트 참여자 | 가입 후 첫 화면까지가 하나의 계약으로 닫혀 있다는 점 |
| `CONVERSATION_AND_MESSAGE_MODEL.md` | 대화/메시지/읽음 상태 모델 | 제품 평가자, 기여자 | 단순 UI가 아니라 메신저 도메인 모델이 있다는 점 |
| `REALTIME_SYNC_AND_CLIENT_STATE.md` | WebSocket, 세션 복구, 클라이언트 상태 | 프런트엔드 기여자, QA | 실시간 반영과 복구 책임 위치가 분명하다는 점 |
| `CLIENT_SURFACES_DESKTOP_AND_WEB.md` | 데스크톱과 웹 표면 전략 | 디자이너, 프런트엔드, 도입 검토자 | 같은 서비스 모델을 다른 표면에 어떻게 번역하는지 |

## 문서별 설계

### 1. `TECHNICAL_ARCHITECTURE.md`

핵심 섹션:

- 전체 지도: `Desktop / Web / Api / Application / Infrastructure / Domain / Contracts`
- 요청 흐름: 입력 -> 엔드포인트 -> 서비스 -> 인프라 -> 응답
- 계층 분리 이유: 공개 계약, 테스트 가능성, 클라이언트 병렬 개발성
- 현재 구조의 한계와 확장 경계

소스 근거:

- `src/PhysOn.Api`
- `src/PhysOn.Application`
- `src/PhysOn.Infrastructure`
- `src/PhysOn.Domain`
- `src/PhysOn.Contracts`
- `src/PhysOn.Desktop`
- `src/PhysOn.Web`

문서가 보여 줄 이점:

- 저장소가 데모가 아니라 제품형 구조를 갖추고 있다는 점
- 신규 기여자가 어디부터 읽어야 하는지 즉시 판단 가능하다는 점

### 2. `AUTH_AND_BOOTSTRAP_FLOW.md`

핵심 섹션:

- 현재 Alpha 가입 방식: `alpha-quick`
- 요청/응답 계약: 표시 이름, 참여 키, 토큰, bootstrap payload
- `refresh token`과 세션 연장 흐름
- bootstrap이 첫 화면에 필요한 데이터를 왜 한 번에 묶는지
- 로컬/운영에서 참여 키가 시드되는 방식

소스 근거:

- `src/PhysOn.Api/Endpoints/MessengerEndpoints.cs`
- `src/PhysOn.Application/Services/MessengerApplicationService.cs`
- `src/PhysOn.Contracts/Auth/AuthContracts.cs`
- `src/PhysOn.Infrastructure/Auth/JwtTokenService.cs`
- `src/PhysOn.Infrastructure/Persistence/DatabaseInitializer.cs`
- `tests/PhysOn.Api.IntegrationTests/VerticalSliceTests.cs`

문서가 보여 줄 이점:

- 공개 독자가 “가입 후 바로 대화까지”의 경로를 이해할 수 있다.
- 테스트 참여자가 참여 키 구조와 현재 범위를 과장 없이 파악할 수 있다.

### 3. `CONVERSATION_AND_MESSAGE_MODEL.md`

핵심 섹션:

- Conversation, Message, ConversationMember의 역할
- 가입 직후 self conversation을 만드는 이유
- 메시지 정렬, 읽음 커서, pinned/unread 상태
- 계약 모델과 UI 표시 모델이 어떻게 이어지는지
- 이후 확장 지점: 파일, 링크, 반응, 검색 인덱스

소스 근거:

- `src/PhysOn.Domain/Conversations/Conversation.cs`
- `src/PhysOn.Domain/Messages/Message.cs`
- `src/PhysOn.Contracts/Conversations/ConversationContracts.cs`
- `src/PhysOn.Application/Services/MessengerApplicationService.cs`

문서가 보여 줄 이점:

- KoTalk가 단순 채팅 껍데기가 아니라, 실제 메시지 도메인 규칙을 갖고 있다는 점
- 기여자가 상태 모델을 잘못 건드리지 않게 해 준다는 점

### 4. `REALTIME_SYNC_AND_CLIENT_STATE.md`

핵심 섹션:

- bootstrap -> realtime ticket -> WebSocket 연결
- 실시간 이벤트 계약과 클라이언트 반영 방식
- 웹 세션 저장과 세션 복구
- 데스크톱 레이아웃/작업 상태 저장
- 재연결과 실패 복구에서 기대하는 현재 동작

소스 근거:

- `src/PhysOn.Contracts/Realtime/RealtimeContracts.cs`
- `src/PhysOn.Api/Endpoints/MessengerEndpoints.cs`
- `src/PhysOn.Web/src/lib/api.ts`
- `src/PhysOn.Web/src/lib/storage.ts`
- `src/PhysOn.Web/src/App.tsx`
- `src/PhysOn.Desktop/ViewModels/MainWindowViewModel.cs`
- `src/PhysOn.Desktop/Services/WorkspaceLayoutStore.cs`

문서가 보여 줄 이점:

- “다시 열었을 때 이어진다”는 체감이 어떤 구조에서 나오는지 설명 가능하다.
- QA가 세션/실시간 이슈를 추적할 때 기준 문서가 생긴다.

### 5. `CLIENT_SURFACES_DESKTOP_AND_WEB.md`

핵심 섹션:

- Desktop와 Web의 역할 차이
- Desktop의 멀티 윈도우와 폭 저장
- Web의 모바일형 정보 위계와 단일 전환 구조
- 두 표면이 공유하는 개념: conversations, messages, bootstrap, reconnect
- 현재 UX 한계와 리팩터링 방향

소스 근거:

- `src/PhysOn.Desktop/Views/MainWindow.axaml`
- `src/PhysOn.Desktop/Views/ConversationWindow.axaml`
- `src/PhysOn.Desktop/ViewModels/MainWindowViewModel.cs`
- `src/PhysOn.Web/src/App.tsx`
- `src/PhysOn.Web/src/App.css`

문서가 보여 줄 이점:

- 독자가 “왜 데스크톱과 웹이 동시에 존재하는가”를 납득할 수 있다.
- 공개 스크린샷이 단순한 이미지 모음이 아니라 표면 전략으로 읽힌다.

## 권장 공개 순서

1. `TECHNICAL_ARCHITECTURE.md`
2. `AUTH_AND_BOOTSTRAP_FLOW.md`
3. `CONVERSATION_AND_MESSAGE_MODEL.md`
4. `REALTIME_SYNC_AND_CLIENT_STATE.md`
5. `CLIENT_SURFACES_DESKTOP_AND_WEB.md`

## 공개 문장 원칙

써야 하는 표현:

- `현재 구현 기준`
- `알파 단계에서 실제 작동하는 흐름`
- `이 저장소에서 확인 가능한 구조`

피해야 하는 표현:

- `완성형 차세대 메신저`
- `카카오톡 완전 대체`
- `엔터프라이즈급 플랫폼 완성`

## 바로 연결할 공개 표면

- `README.md`에는 기술 문서군의 첫 진입점만 둔다.
- `PROJECT_STATUS.md`에는 기술 문서로 이어지는 링크만 둔다.
- 세부 문서는 `문서/` 아래에서 단계적으로 확장한다.
