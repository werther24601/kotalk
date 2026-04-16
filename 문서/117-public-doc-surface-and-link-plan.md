# 공개 문서 표면 및 링크 체인 기획

이 문서는 공개 레포에 기술/보안 문서를 넣을 때 독자가 길을 잃지 않도록 문서 표면을 어떻게 재배치할지 정리한다.

## 기본 정보구조

공개 표면의 기본 축은 아래 다섯 개다.

- `README.md`: 첫 진입
- `PROJECT_STATUS.md`: 현재 상태
- `SECURITY.md`: 보안 허브
- `DEVELOPMENT.md`: 개발 허브
- `RELEASING.md`: 릴리즈 허브

여기에 새 문서군을 추가한다.

- 기술 문서군
  - `문서/TECHNICAL_ARCHITECTURE.md`
  - `문서/AUTH_AND_BOOTSTRAP_FLOW.md`
  - `문서/CONVERSATION_AND_MESSAGE_MODEL.md`
  - `문서/REALTIME_SYNC_AND_CLIENT_STATE.md`
  - `문서/CLIENT_SURFACES_DESKTOP_AND_WEB.md`
- 보안 문서군
  - `문서/APPLIED_SECURITY_CONTROLS.md`
  - `문서/SECURITY_THREAT_MODEL.md`
  - `문서/SECURITY_OPERATING_PRACTICE.md`
  - `문서/SELF_HOSTING_AND_INTERNAL_NETWORK_PROFILE.md`
  - `문서/PRIVACY_AND_DATA_HANDLING_PROFILE.md`
  - `문서/RELEASE_INTEGRITY_AND_TRANSPARENCY.md`
  - `문서/SECURITY_LIMITS_AND_OPEN_GAPS.md`

## README 상단에 고정할 링크 체인

상단 문서 지도에는 아래 다섯 개를 먼저 둔다.

- `프로젝트 한눈에 보기` -> `PROJECT_STATUS.md`
- `보안이 먼저다` -> `SECURITY.md`
- `기술/로직 상세보기` -> `문서/TECHNICAL_ARCHITECTURE.md`
- `로컬 실행/개발` -> `DEVELOPMENT.md`
- `배포 최신 정책` -> `RELEASING.md`

그 아래 “심화 읽기” 구역에서 두 갈래로 나눈다.

- 기술 갈래
  - `TECHNICAL_ARCHITECTURE.md`
  - `AUTH_AND_BOOTSTRAP_FLOW.md`
  - `CONVERSATION_AND_MESSAGE_MODEL.md`
  - `REALTIME_SYNC_AND_CLIENT_STATE.md`
  - `CLIENT_SURFACES_DESKTOP_AND_WEB.md`
- 보안 갈래
  - `APPLIED_SECURITY_CONTROLS.md`
  - `SECURITY_THREAT_MODEL.md`
  - `SECURITY_OPERATING_PRACTICE.md`
  - `SELF_HOSTING_AND_INTERNAL_NETWORK_PROFILE.md`
  - `SECURITY_LIMITS_AND_OPEN_GAPS.md`

## 기존 문서에 넣을 역링크

### `PROJECT_STATUS.md`

- 상태표 아래에 `기술/보안 상세 읽기` 소제목 추가
- `기술 구현 흐름` -> `문서/AUTH_AND_BOOTSTRAP_FLOW.md`
- `실시간과 복구` -> `문서/REALTIME_SYNC_AND_CLIENT_STATE.md`
- `현재 보안 한계` -> `문서/SECURITY_LIMITS_AND_OPEN_GAPS.md`

### `SECURITY.md`

- `빠른 이동` 섹션 추가
- `적용된 통제` -> `문서/APPLIED_SECURITY_CONTROLS.md`
- `위협 모델` -> `문서/SECURITY_THREAT_MODEL.md`
- `운영 보안` -> `문서/SECURITY_OPERATING_PRACTICE.md`
- `릴리즈 검증` -> `문서/RELEASE_INTEGRITY_AND_TRANSPARENCY.md`

### `DEVELOPMENT.md`

- `관련 로직 문서` 섹션 추가
- `가입/세션` -> `문서/AUTH_AND_BOOTSTRAP_FLOW.md`
- `실시간/상태 저장` -> `문서/REALTIME_SYNC_AND_CLIENT_STATE.md`
- `임시 테스트 참여키 운용` -> `문서/SECURITY_OPERATING_PRACTICE.md`

### `RELEASING.md`

- `릴리즈 검증 읽기` 섹션 추가
- `릴리즈 무결성` -> `문서/RELEASE_INTEGRITY_AND_TRANSPARENCY.md`
- `현재 상태` -> `PROJECT_STATUS.md`
- `보안 운영` -> `문서/SECURITY_OPERATING_PRACTICE.md`

## 독자별 읽기 순서

일반 공개 독자:

1. `README.md`
2. `PROJECT_STATUS.md`
3. `SECURITY.md`
4. `RELEASING.md`

기여자:

1. `README.md`
2. `DEVELOPMENT.md`
3. `문서/TECHNICAL_ARCHITECTURE.md`
4. `문서/REALTIME_SYNC_AND_CLIENT_STATE.md`

보안/기관 검토자:

1. `SECURITY.md`
2. `문서/APPLIED_SECURITY_CONTROLS.md`
3. `문서/SECURITY_THREAT_MODEL.md`
4. `문서/SECURITY_OPERATING_PRACTICE.md`
5. `PROJECT_STATUS.md`

## 링크 규칙

- 공개 문서는 상대경로를 우선한다.
- 같은 문서에 링크를 과하게 나열하지 않는다. `관련 문서`는 3개 정도로 제한한다.
- `README.md`에서는 절대 URL보다 repo-relative 링크를 우선 사용한다.
- 기술 문서와 보안 문서는 서로 링크하되, 역할이 겹치지 않게 한다.

## 공개 표면에서 피할 것

- 내부 전략 메모 성격의 문장
- 비밀값, 실제 운영 참여키, 관리자 주소
- “완벽”, “근본적 해결”, “기관급 충족” 같은 단정형 표현

## 바로 다음 단계

- 기획 문서를 먼저 `문서/`에 정리
- 이후 공개 문서 초안을 루트 문서와 `문서/`에 나눠 도입
- README / SECURITY / DEVELOPMENT / RELEASING 역링크까지 함께 정리
