# 112. Technical Operations Release Trust Atlas

이 문서는 현재 `KoTalk` 문서 세트를 기준으로, 다음 확장 라운드에서 필요한 `기술/운영/릴리즈/신뢰` 설계 문서를 아틀라스 형태로 정리한다.

목표는 단순히 문서 수를 늘리는 것이 아니라, 아래 네 가지를 문서 단위로 분리해 구현과 운영의 기준점으로 쓰는 것이다.

- 기능 설계와 운영 설계를 분리한다.
- 제품 UX 문서와 API/인프라 문서를 연결한다.
- 릴리즈와 다운로드, 관찰성, 복구를 `실서비스 품질` 기준으로 끌어올린다.
- Android 병렬 채널, 공개 다운로드, 원격 Releases, 운영 복구를 하나의 체계로 묶는다.

## 아틀라스 구조

아래 8개 범주로 문서를 확장한다.

1. 검색과 재발견
2. 보관함과 장기 보존
3. 세션/인증/디바이스 연속성
4. 오프라인/동기화/전송 복구
5. 릴리즈/다운로드/멀티채널 배포
6. 운영 복구/로그/관찰성
7. API 계약/버전/이벤트 정합성
8. 권한/보안/신뢰 통제

각 범주 안에서 `제품 UX 명세`, `시스템 계약`, `운영 플레이북`, `QA/릴리즈 게이트`까지 최소 4층 구조를 갖는 것이 이상적이다.

## 현재 문서 세트의 상태 요약

이미 존재하는 강한 축:

- 검색 UX 철학과 결과 표현: `24`, `58`, `69`
- 세션/복구 UX: `25`, `27`, `51`, `79`, `104`, `109`
- 오프라인/아웃박스 규칙: `78`
- Android 병렬 전략/배포 표면: `15`, `30`, `54`, `110`
- 공개 릴리즈 표면/스크린샷: `45`, `55`, 루트 `RELEASING.md`
- 운영/지원/신뢰 언어: `39`, `47`, `57`, `98`

아직 부족한 축:

- 검색을 실제 인덱스/랭킹/권한/증분 갱신 구조까지 내린 기술 설계
- 보관함의 저장소 모델, 수명주기, 사용자 제어, 다운로드/내보내기 설계
- 세션을 `토큰-디바이스-브라우저-복구 정책`까지 분리한 계약 문서
- 오프라인 큐 충돌 해결과 재동기화 정책의 기술 명세
- 공개 다운로드/원격 Releases/버전 메타데이터의 일관성 표준
- 운영 복구에서 `무엇을 보고 어떻게 판단하는가`를 정한 관찰성 문서
- REST/WSS/배치 작업/릴리즈 메타 파일을 함께 묶는 API 버전 정책
- 권한, 감사 로그, 관리자 액션, 비밀정보 보관, 다운로드 서명 검증 설계

## 범주별 신규 문서 제안

### 1. 검색과 재발견

#### 113-search-architecture-indexing-and-ranking-spec.md
- 목적: 검색 UX 문서를 실제 시스템 구조로 연결한다.
- 다룰 내용:
  - 대화/메시지/파일/링크/사용자 인덱스 구조
  - 증분 인덱싱과 백필 정책
  - 최근성, 고정, 안읽음, 참여도 기반 랭킹
  - 권한 필터와 비공개 대화 가시성
  - 모바일/데스크톱 검색 응답 축약 규칙

#### 114-search-query-contract-saved-searches-and-analytics.md
- 목적: 검색 입력, 필터, 저장 검색, 최근 검색, 분석 이벤트를 하나의 계약으로 묶는다.
- 다룰 내용:
  - 쿼리 파서 규칙
  - 저장 검색의 사용자 모델
  - 검색 실패/무결과/제안어 정책
  - 검색 성공률/첫 클릭 시간 지표

#### 115-search-qa-relevance-benchmark-and-golden-datasets.md
- 목적: 검색 품질을 회귀 테스트 가능하게 만든다.
- 다룰 내용:
  - 골든 쿼리 세트
  - 업무형/친근형 검색 시나리오
  - 정답 문서 집합과 허용 오차
  - 릴리즈 전 relevance gate

### 2. 보관함과 장기 보존

#### 116-vault-information-architecture-and-user-mental-model.md
- 목적: `보관함`을 단순 북마크 묶음이 아니라 목적지로 정의한다.
- 다룰 내용:
  - 파일, 링크, 북마크, 나중에 답장, 저장 메시지의 섹션 구조
  - 사용자 관점 명명 규칙
  - 모바일/데스크톱 탐색 차이

#### 117-vault-storage-lifecycle-export-and-retention-spec.md
- 목적: 보관함 데이터의 저장/삭제/내보내기 정책을 정한다.
- 다룰 내용:
  - 영구 저장과 임시 저장의 경계
  - 자동 만료와 사용자 수동 삭제
  - 내려받기, ZIP 묶음, 링크 만료 정책
  - MinIO 객체 보관 규칙과 메타데이터

#### 118-vault-permissions-sharing-and-sensitive-content-policy.md
- 목적: 보관함 내 공유와 민감 콘텐츠 규칙을 명확히 한다.
- 다룰 내용:
  - 개인 저장 vs 대화 기반 저장
  - 링크 재공유 권한
  - 민감 파일 마스킹과 관리자 접근 제한

### 3. 세션/인증/디바이스 연속성

#### 119-session-token-device-and-recovery-architecture.md
- 목적: 현재 세션 UX 문서를 인증 시스템 계약으로 내린다.
- 다룰 내용:
  - access/refresh/session ticket 구조
  - 디바이스 식별자와 브라우저 세션 차이
  - 회전, 만료, 폐기, 재발급 정책
  - 마지막 정상 화면 유지 조건

#### 120-auth-journey-matrix-by-channel-web-windows-android.md
- 목적: Web/Windows/Android의 가입/로그인/복구 차이를 한 표로 정리한다.
- 다룰 내용:
  - 채널별 로그인 진입
  - 초대코드, 이메일 확인, 추후 passkey 확장 경로
  - 채널별 오류 문구, 재시도, 세션 정리 위치

#### 121-device-management-remote-signout-and-risk-controls.md
- 목적: 내 공간의 기기 관리와 원격 로그아웃 정책을 정한다.
- 다룰 내용:
  - 등록 기기 목록
  - 마지막 활동 시각
  - 의심 로그인 감지
  - 사용자 알림과 차단/해제 흐름

### 4. 오프라인/동기화/전송 복구

#### 122-offline-sync-engine-conflict-resolution-spec.md
- 목적: 오프라인 규칙을 실제 동기화 엔진 설계로 구체화한다.
- 다룰 내용:
  - local-first state
  - sync cursor와 증분 복구
  - 충돌 해결 우선순위
  - 전송/수정/읽음 이벤트 재정렬 규칙

#### 123-outbox-retry-idempotency-and-message-dedup-policy.md
- 목적: 전송 실패 복구와 중복 메시지 방지를 계약으로 묶는다.
- 다룰 내용:
  - idempotency key
  - retry backoff
  - 중복 감지 기준
  - 사용자 표면의 실패/재전송 상태

#### 124-offline-qa-lab-network-fault-injection-playbook.md
- 목적: 오프라인/불안정 네트워크를 재현하는 QA 문서를 만든다.
- 다룰 내용:
  - 2G/패킷 손실/짧은 단절/장시간 오프라인
  - 모바일 브라우저 백그라운드/복귀
  - Windows 재기동 후 복구

### 5. 릴리즈/다운로드/멀티채널 배포

#### 125-release-metadata-schema-and-version-manifest-contract.md
- 목적: 웹, Windows, Android, 다운로드 호스트, 원격 Releases를 하나의 버전 메타 파일로 연결한다.
- 다룰 내용:
  - `version.json` 스키마
  - commit SHA, build date, artifact URL, checksum, screenshot set
  - latest/previous/stable channel 정의

#### 126-download-host-routing-signing-and-integrity-spec.md
- 목적: `download-vstalk.phy.kr`를 릴리즈 인프라로 정의한다.
- 다룰 내용:
  - `/windows/latest`, `/android/latest`, `/releases/<version>/...`
  - HTTPS, MIME, 캐시 전략
  - checksum, signature, manifest 제공 방식
  - 손상 파일/잘못된 latest 포인터 복구 절차

#### 127-gitea-releases-artifact-publishing-and-mirroring-runbook.md
- 목적: 원격 Releases와 다운로드 미러 게시를 같은 절차로 운영한다.
- 다룰 내용:
  - 태그 생성
  - 릴리즈 노트
  - 자산 업로드
  - 실패 시 재게시와 롤백

#### 128-android-parallel-channel-release-governance.md
- 목적: Android 채널을 `병렬이되 종속적이지 않게` 운영하는 기준을 만든다.
- 다룰 내용:
  - Windows/Web/Android 버전 정렬 정책
  - APK universal/split 전략
  - Android 스크린샷/체크섬/권한 공지 기준
  - 채널 간 known gaps 공개 원칙

### 6. 운영 복구/로그/관찰성

#### 129-operational-observability-signal-map.md
- 목적: 운영자가 무엇을 봐야 하는지 정의한다.
- 다룰 내용:
  - API, WSS, DB, Redis, MinIO, Caddy, download host 핵심 지표
  - 세션 재발급 실패율, 연결 수, 오류율, 메시지 지연
  - 제품 상태 페이지와 내부 대시보드 연결

#### 130-log-schema-redaction-and-retention-policy.md
- 목적: 로그를 남기되 과수집하지 않는 기준을 만든다.
- 다룰 내용:
  - 구조화 로그 필드
  - PII 마스킹
  - trace/correlation ID
  - 보관 기간과 삭제 정책

#### 131-incident-response-runbook-and-service-recovery-tiers.md
- 목적: 장애 대응을 단계형으로 정리한다.
- 다룰 내용:
  - Sev 등급
  - 최초 감지, 공지, 완화, 복구, 사후 보고
  - 세션 장애, DB 장애, 다운로드 호스트 장애, 잘못된 릴리즈 배포별 플레이북

#### 132-backup-restore-drills-and-disaster-recovery-verification.md
- 목적: 백업과 복구를 문서가 아니라 실제 연습 대상으로 만든다.
- 다룰 내용:
  - PostgreSQL/MinIO 백업
  - 릴리즈 자산 백업
  - 월간 복구 훈련 체크리스트

### 7. API 계약/버전/이벤트 정합성

#### 133-rest-api-versioning-error-envelope-and-pagination-contract.md
- 목적: 현재 API 문서를 릴리즈 가능한 계약으로 정리한다.
- 다룰 내용:
  - 버전 정책
  - 공통 오류 envelope
  - cursor pagination
  - rate limit 헤더

#### 134-websocket-event-taxonomy-delivery-order-and-replay-spec.md
- 목적: 실시간 이벤트의 종류와 전달 순서를 명확히 한다.
- 다룰 내용:
  - message, receipt, typing, presence, system event 분류
  - ordering, ack, replay, reconnect cursor
  - 중복 수신과 누락 복구

#### 135-client-server-capability-negotiation-and-feature-flags.md
- 목적: 채널별 구현 차이를 API로 안전하게 흡수한다.
- 다룰 내용:
  - capability handshake
  - experimental feature flags
  - 최소 지원 버전
  - Android/Web/Windows 기능 차이 공개 원칙

### 8. 권한/보안/신뢰 통제

#### 136-role-permission-matrix-and-admin-boundaries.md
- 목적: 일반 사용자, 방 관리자, 서비스 운영자 권한을 분리한다.
- 다룰 내용:
  - 대화방 수준 권한
  - 관리자 도구 접근 범위
  - 읽기/삭제/추방/공지 권한

#### 137-secret-management-key-rotation-and-build-signing-policy.md
- 목적: 운영 비밀정보와 산출물 서명 정책을 정한다.
- 다룰 내용:
  - `.env` 제거 계획
  - 비밀정보 저장소
  - 키 회전 주기
  - Windows/Android 빌드 서명 자격증명 관리

#### 138-audit-trail-sensitive-actions-and-user-visible-history.md
- 목적: 민감 액션의 감사 추적과 사용자 가시성을 정한다.
- 다룰 내용:
  - 관리자 액션 로그
  - 원격 로그아웃
  - 다운로드 게시/삭제
  - 사용자에게 보여줄 이력 범위

#### 139-security-review-checklist-and-release-gates.md
- 목적: 보안 검토를 릴리즈 전에 필수화한다.
- 다룰 내용:
  - 인증/권한/로그/스토리지/다운로드 서명 체크리스트
  - 취약점 triage
  - 보안 회귀 테스트

## 우선순위

### P0

이 단계는 실서비스 신뢰와 직접 연결된다.

- 119-session-token-device-and-recovery-architecture.md
- 122-offline-sync-engine-conflict-resolution-spec.md
- 125-release-metadata-schema-and-version-manifest-contract.md
- 126-download-host-routing-signing-and-integrity-spec.md
- 129-operational-observability-signal-map.md
- 133-rest-api-versioning-error-envelope-and-pagination-contract.md
- 134-websocket-event-taxonomy-delivery-order-and-replay-spec.md
- 137-secret-management-key-rotation-and-build-signing-policy.md

### P1

이 단계는 업무형 사용성과 운영 반복성을 크게 끌어올린다.

- 113-search-architecture-indexing-and-ranking-spec.md
- 116-vault-information-architecture-and-user-mental-model.md
- 117-vault-storage-lifecycle-export-and-retention-spec.md
- 120-auth-journey-matrix-by-channel-web-windows-android.md
- 123-outbox-retry-idempotency-and-message-dedup-policy.md
- 127-gitea-releases-artifact-publishing-and-mirroring-runbook.md
- 131-incident-response-runbook-and-service-recovery-tiers.md
- 136-role-permission-matrix-and-admin-boundaries.md
- 138-audit-trail-sensitive-actions-and-user-visible-history.md

### P2

이 단계는 운영 성숙도와 장기 확장성을 높인다.

- 114-search-query-contract-saved-searches-and-analytics.md
- 115-search-qa-relevance-benchmark-and-golden-datasets.md
- 118-vault-permissions-sharing-and-sensitive-content-policy.md
- 121-device-management-remote-signout-and-risk-controls.md
- 124-offline-qa-lab-network-fault-injection-playbook.md
- 128-android-parallel-channel-release-governance.md
- 130-log-schema-redaction-and-retention-policy.md
- 132-backup-restore-drills-and-disaster-recovery-verification.md
- 135-client-server-capability-negotiation-and-feature-flags.md
- 139-security-review-checklist-and-release-gates.md

## 권장 작성 순서

1. 세션, API, WSS, 오프라인, 다운로드 메타 계약부터 정리한다.
2. 그 다음 관찰성/로그/장애 복구 플레이북을 묶는다.
3. 이후 검색/보관함/Android 채널을 제품+시스템 양면으로 확장한다.
4. 마지막으로 QA 골든셋과 보안 릴리즈 게이트를 붙여 반복 운영 체계를 완성한다.

## 전문 관점별 소유권 제안

- Product Manager: `116`, `120`, `128`
- UX Researcher: `113`, `114`, `116`, `118`
- API Designer: `119`, `133`, `134`, `135`
- Infrastructure/SRE: `125`, `126`, `127`, `129`, `131`, `132`
- Security Auditor: `136`, `137`, `138`, `139`
- QA Lead: `115`, `124`, `139`

## 결론

다음 문서 확장은 `아이디어 문서`를 더 쌓는 방향보다, 이미 잡힌 UX와 구현을 `신뢰 가능한 시스템/운영/릴리즈 계약`으로 내리는 방향이어야 한다.

특히 지금 시점의 가장 큰 공백은 아래 네 가지다.

- 검색이 강한 UX 기획에 비해 기술 인덱스/랭킹 문서가 약하다.
- 보관함이 목적지로 도입됐지만 저장/권한/보존 수명주기 문서가 없다.
- 세션과 오프라인 복구는 UX 원칙은 있으나 토큰/이벤트/충돌 해결 계약이 분리돼 있지 않다.
- 릴리즈와 다운로드가 실제 서비스 표면이 되었는데도, 메타데이터/서명/미러링/복구 표준이 아직 독립 문서로 부족하다.

따라서 다음 라운드 문서 확장은 `검색`, `세션`, `오프라인`, `릴리즈 메타`, `관찰성`, `API`, `권한/보안`을 중심으로 진행하는 것이 가장 효율적이다.
