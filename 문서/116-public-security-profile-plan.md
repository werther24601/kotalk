# 공개 보안 문서 프로파일 기획

이 문서는 공개 레포에 도입할 `보안 관련 문서군`의 설계안이다.

핵심 원칙은 명확하다.

- 보안 문서는 슬로건이 아니라 `이미 코드에 들어간 통제`를 설명해야 한다.
- `지금 되는 것`, `제한된 것`, `향후 계획`을 같은 문단에 섞지 않는다.
- “안전하다”보다 “무엇을 했고, 무엇을 아직 하지 않았는가”를 분명히 적는다.

## 현재 코드 기준 보안 주장선

공개 문서는 아래 수준까지만 주장한다.

| 영역 | 현재 코드에서 말할 수 있는 것 | 아직 말하면 안 되는 것 | 근거 파일 |
|---|---|---|---|
| 자체 구축 / 내부망 | `자체 호스팅과 내부망 전용 배포가 가능하도록 설계된 구조` | `기관망 즉시 적합`, `공공기관 수준 검증 완료` | `deploy/compose.mvp.yml`, `deploy/Caddyfile` |
| 세션 / 토큰 | `짧은 수명 access token`, `회전형 refresh token`, `서버의 세션 재검증` | `device binding 완성`, `zero trust 완비` | `src/PhysOn.Infrastructure/ServiceCollectionExtensions.cs`, `src/PhysOn.Infrastructure/Auth/JwtTokenService.cs`, `src/PhysOn.Api/Endpoints/MessengerEndpoints.cs` |
| 참여 키 흐름 | `알파 단계의 제한된 참여 키 기반 온보딩` | `정식 신원 검증`, `abuse 방어 완비` | `src/PhysOn.Application/Services/MessengerApplicationService.cs`, `src/PhysOn.Infrastructure/Persistence/DatabaseInitializer.cs` |
| 릴리즈 무결성 | `공식 다운로드 경로와 SHA-256 체크섬 제공` | `공급망 보안 완결`, `signed build 완비` | `scripts/release/release-prepare-assets.sh`, `scripts/release/release-upload-assets.sh` |
| 전송 계층 | `TLS 기반 공식 운영 경로`, `기본 보안 헤더`, `wss 문맥 보정` | `모든 고급 네트워크 공격 방어` | `deploy/Caddyfile`, `src/PhysOn.Mobile.Android/AndroidManifest.xml`, `src/PhysOn.Web/src/lib/realtime.ts` |
| 비밀값 처리 | `env 기반 비밀값 주입`, `운영 기본 키 차단`, `Windows DPAPI 보호` | `중앙 secret manager 완비`, `키 회전 체계 완비` | `deploy/.env.example`, `src/PhysOn.Infrastructure/ServiceCollectionExtensions.cs`, `src/PhysOn.Desktop/Services/SessionStore.cs` |
| 클라이언트 신뢰 경계 | `웹/데스크톱/Android가 서로 다른 저장 경계를 가진다` | `모든 클라이언트가 동일한 보안 수준` | `src/PhysOn.Web/src/lib/storage.ts`, `src/PhysOn.Desktop/Services/SessionStore.cs`, `src/PhysOn.Mobile.Android/MainActivity.cs` |

## 공개 보안 문서군의 기본 구조

| 프로파일 | 공개 문서 후보 | 주 독자 | 핵심 목적 |
|---|---|---|---|
| 보안 개요 | `SECURITY_OVERVIEW.md` | 모든 공개 독자 | 현재 보안 자세와 허용 가능한 주장선 설명 |
| 운영 프로파일 | `SECURITY_PROFILES.md` | 인프라 담당자, 기관/사내 검토자 | 공개 배포 / 자체 구축 / 내부망 / 로컬 개발을 구분 |
| 세션/인증 모델 | `AUTH_AND_SESSION_MODEL.md` | 백엔드, QA, 기술 검토자 | 참여 키, 토큰, 세션, realtime ticket 흐름 설명 |
| 전송/릴리즈 신뢰 | `TRANSPORT_AND_RELEASE_TRUST.md` | 다운로드 사용자, 운영자 | TLS, 헤더, 공식 다운로드, 체크섬 설명 |
| 클라이언트 신뢰 경계 | `CLIENT_TRUST_BOUNDARY.md` | 사용자, 평론가, 보안 검토자 | 웹/데스크톱/Android의 보호 경계 차이를 설명 |
| 적용된 통제 | `APPLIED_SECURITY_CONTROLS.md` | 일반 사용자, 기술 검토자 | 현재 코드에 실제로 들어간 보안 통제를 설명 |
| 위협 모델 | `SECURITY_THREAT_MODEL.md` | 보안 담당자, 기술 도입 검토자 | 자산, 공격면, 완화 조치를 구조적으로 설명 |
| 운영 보안 | `SECURITY_OPERATING_PRACTICE.md` | 운영자, 기관/사내 검토자 | 키, 토큰, 배포, 로그, 사고 대응 원칙 설명 |
| 자체 구축/내부망 | `SELF_HOSTING_AND_INTERNAL_NETWORK_PROFILE.md` | 인프라 담당자, 기관 검토자 | SaaS 외 운영 가능성과 현재 한계 설명 |
| 프라이버시/데이터 | `PRIVACY_AND_DATA_HANDLING_PROFILE.md` | 일반 사용자, 평론가 | 무엇이 서버에 있고, 무엇이 로컬에 있는지 설명 |
| 릴리즈 무결성 | `RELEASE_INTEGRITY_AND_TRANSPARENCY.md` | 다운로드 사용자, 오픈소스 기여자 | 태그, 자산, 체크섬, 미러 구조 설명 |
| 한계 문서 | `SECURITY_LIMITS_AND_OPEN_GAPS.md` | 모든 공개 독자 | 아직 아닌 것과 향후 과제를 정직하게 분리 |

## 프로파일별 설계

### 0. 보안 개요

핵심 메시지:

- KoTalk의 공개 보안 문서는 `무엇을 이미 구현했는지`, `무엇을 아직 주장하지 않는지`, `어디까지가 운영자 책임인지`를 먼저 보여 줘야 한다.

소스 근거:

- `src/PhysOn.Api`
- `src/PhysOn.Infrastructure`
- `deploy/`
- `scripts/release/`

반드시 포함할 축:

- Scope And Security Posture
- What KoTalk Implements Today
- What Is Explicitly Not Claimed Yet
- Current Security Boundaries
- Reporting A Vulnerability

### 1. 적용된 통제

핵심 메시지:

- KoTalk는 알파 기준에서도 `JWT 서명 검증`, `세션 활성 검증`, `인증/실시간 rate limit`, `민감 응답 no-store`, `로컬 세션 보호` 같은 통제를 코드에 갖고 있다.

소스 근거:

- `src/PhysOn.Infrastructure/ServiceCollectionExtensions.cs`
- `src/PhysOn.Api/Program.cs`
- `src/PhysOn.Api/Endpoints/MessengerEndpoints.cs`
- `src/PhysOn.Desktop/Services/SessionStore.cs`
- `tests/PhysOn.Api.IntegrationTests/VerticalSliceTests.cs`

공개 문장에서 금지할 표현:

- `군사급 보안`
- `정보유출 불가능`
- `기관 등급 충족`
- `완전한 종단간 암호화`

### 2. 위협 모델

핵심 메시지:

- 자산은 `세션`, `토큰`, `메시지`, `invite`, `릴리즈 산출물`, `운영 비밀값`으로 나뉘고, 각 자산의 공격면과 완화 조치가 다르다.

다뤄야 할 공격면:

- invite 남용
- refresh token 탈취
- WebSocket misuse
- 다운로드 미러 위조
- 운영 환경의 비밀값 누출
- 클라이언트 로컬 세션 탈취

공개 문장에서 금지할 표현:

- `완전한 위협 차단`
- `모든 공격 시나리오 대응 완료`

### 3. 운영 보안

핵심 메시지:

- KoTalk는 운영 환경에서 `환경변수 기반 비밀값`, `키 교체 가능성`, `세션/토큰 분리`, `릴리즈 체크섬` 같은 운영 원칙을 요구한다.

다뤄야 할 항목:

- JWT issuer/audience/signing key
- bootstrap invite seed
- 비밀값을 공개 문서에 두지 않는 기준
- 취약점 제보와 사고 대응 흐름
- 로그에 메시지 본문을 기본값으로 남기지 않는 원칙

소스 근거:

- `deploy/compose.mvp.yml`
- `deploy/.env.example`
- `src/PhysOn.Api/appsettings.json`
- `src/PhysOn.Api/appsettings.Development.json`

### 4. 자체 구축 / 내부망

핵심 메시지:

- 현재 KoTalk는 `단일 API + reverse proxy + 환경변수 + 파일 기반 DB` 구조로 작동하는 작은 배포 단위를 갖고 있어 내부망 PoC나 자체 운영 검토에 유리하다.

반드시 같이 적을 제한:

- 현재는 SQLite 단일 노드 MVP
- 기관망/폐쇄망 검증 완료 상태는 아님
- 대규모 HA/DR, 다중 리전 같은 운영 수준은 아직 문서화 단계가 아님

소스 근거:

- `deploy/compose.mvp.yml`
- `src/PhysOn.Api/Program.cs`
- `src/PhysOn.Infrastructure/Persistence/VsMessengerDbContext.cs`

### 5. 프라이버시 / 데이터 처리

핵심 메시지:

- 지금 단계의 KoTalk는 무엇을 수집하고 어디에 저장하는지 설명 가능한 작은 표면을 갖고 있다.

반드시 포함할 문장:

- 현재는 E2EE가 아니다.
- 서버 저장 구조가 존재한다.
- 운영자가 책임지는 영역이 있다.
- Windows 로컬 세션 저장과 웹 세션 저장의 경계가 다르다.

소스 근거:

- `src/PhysOn.Application/Services/MessengerApplicationService.cs`
- `src/PhysOn.Desktop/Services/SessionStore.cs`
- `src/PhysOn.Web/src/lib/storage.ts`
- `src/PhysOn.Mobile.Android/MainActivity.cs`

금지할 표현:

- `추적하지 않는다` 단정
- `메타데이터를 남기지 않는다`
- `프라이버시 완전 보호`

### 6. 릴리즈 무결성과 운영 투명성

핵심 메시지:

- 릴리즈는 `버전`, `자산`, `체크섬`, `다운로드 미러`, `릴리즈 노트` 기준으로 추적 가능해야 한다.

소스 근거:

- `scripts/release/release-prepare-assets.sh`
- `scripts/release/release-publish-forge.sh`
- `scripts/release/release-publish-github.sh`
- `release-assets/`
- `artifacts/builds/`

금지할 표현:

- `공급망 공격 차단 완료`
- `릴리즈 무결성 완전 보장`

### 7. 한계와 열린 과제

핵심 메시지:

- KoTalk는 지금 어디까지 구현됐고, 무엇은 아직 아니다.

반드시 적어야 할 현재 갭:

- Android는 현재 WebView 셸
- 가입은 아직 참여 키 기반 alpha flow
- 단일 DB / 단일 API MVP
- E2EE 미구현
- 기관 운영 검증 미완료

## 기존 공개 문서와의 연결

- `SECURITY.md`는 계속 허브 문서로 유지한다.
- `TRUST_CENTER.md`는 요약/표면 문서로 두고, 세부 보안 프로파일 문서로 분기시킨다.
- `PRIVACY_AND_DATA_HANDLING.md`가 이미 있다면, 새 프로파일 문서와 역할을 분리한다.
- `RELEASING.md`는 릴리즈 무결성 문서와 상호 링크한다.
- `DEVELOPMENT.md`는 테스트 참여 키, 로컬 운영, 비밀값 주입 관련 링크만 남기고 세부 보안 설명은 새 문서군으로 보낸다.

## 공개 편집 원칙

써야 하는 표현:

- `현재 구현 기준`
- `알파 단계에서 실제 적용된 통제`
- `운영 환경에서는 ...를 요구`
- `이 저장소에서 확인 가능한 근거`

피해야 하는 표현:

- `근본적으로 해결했다`
- `완벽히 안전하다`
- `완전한 탈중앙화`
- `기관 수준 보안 충족`
- `카카오톡 대체 완성`

## 우선 공개 순서

1. `SECURITY_OVERVIEW.md`
2. `AUTH_AND_SESSION_MODEL.md`
3. `TRANSPORT_AND_RELEASE_TRUST.md`
4. `CLIENT_TRUST_BOUNDARY.md`
5. `SECURITY_PROFILES.md`
6. `APPLIED_SECURITY_CONTROLS.md`
7. `SECURITY_THREAT_MODEL.md`
8. `SECURITY_OPERATING_PRACTICE.md`
9. `SELF_HOSTING_AND_INTERNAL_NETWORK_PROFILE.md`
10. `PRIVACY_AND_DATA_HANDLING_PROFILE.md`
11. `RELEASE_INTEGRITY_AND_TRANSPARENCY.md`
12. `SECURITY_LIMITS_AND_OPEN_GAPS.md`
