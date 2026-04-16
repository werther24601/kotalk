# Changelog

이 프로젝트는 현재 초기 설계 단계를 넘어 첫 실행 가능한 Alpha 프로토타입 단계에 들어갔습니다.

모든 의미 있는 변경은 이 파일에 기록합니다.

## [Unreleased]

### Added

- Android WebView 기반 첫 APK 프로젝트와 `KoTalk-android-universal-*.apk` 산출물 경로 추가
- Windows `installer exe + onefile exe + zip` 3종 배포 체인 추가
- 모바일/iOS/Linux 프레임워크 결론 문서 `CLIENT_PLATFORM_DECISION.md` 추가
- 공개 원격용 버전 태그 생성 스크립트 `scripts/release/release-create-tag.sh`
- GitHub 릴리즈 게시 스크립트 `scripts/release/release-publish-github.sh`
- 제2·제3 공개 원격 순차 게시 스크립트 `scripts/release/release-publish-public.sh`

- `artifacts/builds/2026.04.16-alpha.4/` Windows 포터블 산출물과 체크섬 추가
- `download-vstalk.phy.kr` HTTPS 미러를 실제 latest ZIP/manifest 경로로 활성화

- `KoTalk` 공개 브랜드 기준과 다운로드/릴리즈 표면 정리
- 공개 가입 전략을 `1회성 인증 중심`으로 재정의한 기획 문서 보강
- Apache-2.0 기준의 라이선스/상표/기여 정책 정리
- 공개 루트 문서 전면 개편과 다운로드 경로 하이퍼링크 정리

- 한국어 Windows 메신저 프로젝트 방향 수립
- `문서/` 기준의 마스터 기획 세트 작성
- 최근 국내 카카오톡 여론을 반영한 프로젝트 배경/시장 맥락 문서 추가
- `MIT License`, `CODE_OF_CONDUCT.md`, `DEVELOPMENT.md`, `ARCHITECTURE.md`, `ROADMAP.md`, `SUPPORT.md` 추가
- Android 병렬 채널 전략 문서 추가
- Forge Releases 게시 스크립트 추가
- 한국어 UI 문체 시스템 문서 추가
- 가입/온보딩/인증 정책 문서 추가
- 카카오톡 PC 패리티/상위호환 매트릭스 문서 추가
- 공개 저장소용 `README.md`, `CONTRIBUTING.md`, `SECURITY.md`, `BRANCHING_STRATEGY.md` 추가
- 릴리즈 다운로드 도메인 정책 반영: `https://download-vstalk.phy.kr`
- `ASP.NET Core + JWT + EF Core + SQLite + WebSocket` 기반 Alpha 서버 수직 슬라이스
- `Avalonia 12 + .NET 8` 기반 Windows 데스크톱 Alpha 셸
- `v0.1.0-alpha.1` Windows x64 portable zip 산출물
- 릴리즈 번들 메타데이터, 체크섬, 스크린샷 생성 규약
- VPS용 MVP 배포 스캐폴딩, Caddy 예시, 릴리즈 업로드 스크립트
- `https://vstalk.phy.kr` 모바일 웹앱 실배포와 same-origin API 운영 경로
- `PROJECT_STATUS.md`, `GOVERNANCE.md`, 저장소 전용 README 시각 자산 추가
- `문서/18-white-material-compact-ui-system.md`, `문서/19-desktop-adaptive-window-and-multiwindow-guidelines.md`, `문서/20-kakao-public-pattern-benchmark-and-vs-translation.md` 추가
- 사용자 여정별 점검 기준과 QA 문서 주제 강화를 위한 `문서/63-user-journey-review-framework-and-qa-topics.md` 추가
- `COMMUNITY.md`, `MAINTAINERS.md`, `RELEASING.md`, `FIRST_CONTRIBUTION.md` 추가
- README 전용 공개 저장소 시각 자산 `open-source-surface.svg`, `contribution-path.svg` 추가
- UX 중심 저장소 표면을 위한 `ux_review` 이슈 템플릿 추가
- 사용자 관점 리뷰와 비판적 QA 범주 확장을 위한 `문서/112-review-surface-expansion-and-critical-qa-proposal.md` 추가
- 루트 120개 문서와 세부 아틀라스 253개 문서로 구성된 `문서/atlas/` 확장 세트 추가
- 공개 저장소 첫 진입을 위한 `FAQ.md`, `SHOWCASE.md` 추가
- README 전용 공개 표면 자산 `public-contract.svg`, `evaluation-paths.svg` 추가
- 공개 사업모델 기준 문서 `BUSINESS_MODEL.md`, `문서/113-open-core-platform-business-and-procurement-strategy.md` 추가
- 핵심 차별점 고정 문서 `문서/114-core-differentiation-pillars.md` 추가
- `TRUST_CENTER.md`, `SECURITY_RESPONSE.md`, `DEPLOYMENT_MODES.md`, `PRIVACY_AND_DATA_HANDLING.md`, `PROCUREMENT_READINESS.md`, `PORTFOLIO_CAPABILITIES.md`, `TRADEMARKS.md`, `CONTRIBUTOR_LICENSE_POLICY.md`, `LICENSE-FAQ.md` 추가

### Changed

- 공개 릴리즈 Assets에서 스크린샷과 릴리즈 노트 첨부를 제거하고, 최신 화면은 변경 노트 본문에서 직접 확인하도록 조정
- Android 상태를 `계획`에서 `첫 APK 기준선 확보` 단계로 상향하고, iOS/Linux 계획을 공개 문서에 명시
- 데스크톱/웹 스크린샷 캡처를 앱 창·앱 셸 기준으로 정리하고, README·SHOWCASE 이미지를 실제 종횡비에 맞춘 `width/height` 기준으로 재배치
- 공개 원격 배포 정책을 `public/* 브랜치 + 버전 태그 + 릴리즈 페이지 + 자산` 기준으로 고정
- Gitea/GitHub 릴리즈 게시 스크립트가 지정 원격 기준으로 동작하고 최신 스크린샷 자산도 함께 첨부하도록 확장
- 비-`origin` 원격에 대한 로컬 pre-push 가드가 `public/*` 브랜치와 `refs/tags/*`를 함께 허용하도록 조정

- 데스크톱·웹 UI의 설명형 카피를 더 줄이고 말풍선/칩/버튼 라운드를 2px 기준으로 축소
- 웹 앱 버전을 `0.1.0-alpha.4`로 올리고 최신 캡처 자산을 재생성
- 다운로드 스크립트, 릴리즈 워크플로, Caddy 예시 설정을 `download-vstalk.phy.kr`와 `KoTalk-*` 자산명 기준으로 정렬
- 다운로드 미러 상태를 DNS/HTTPS 정상 동작 기준으로 문서에 반영

- 공개 브랜드를 `KoTalk`로 정리하고 공개 문서의 직접적·내부지향 표현을 제거
- README를 대중용 첫인상 기준으로 다시 구성하고 다운로드는 공식 미러와 저장소 릴리즈를 함께 표기
- 보안/신뢰 문서에서 운영 힌트와 공유 접근값 노출을 줄이고 공개 범위를 재정의
- 라이선스를 Apache-2.0으로 정리하고 일반 기여의 기본 규칙을 단순화
- 공개 가입 정책을 `초대코드 중심`에서 `이메일/휴대폰 기반 1회성 인증` 방향으로 수정
- 디자인 지침을 `각진, 플랫, 텍스트 최소화, 머터리얼 계열` 원칙으로 보강

- 제품 방향을 `복제형`이 아니라 `한국어 Windows 메신저 최적화`로 명확히 조정
- 가입 정책을 `Alpha 즉시 실행형`과 `Beta 기본형`으로 분리
- README와 마스터 문서 세트 전면 보강
- 최근 기사와 공개 자료를 바탕으로 프로젝트 배경 설명을 신사적 톤으로 재구성
- README를 스크린샷, 빠른 시작, 아키텍처, 로드맵 중심의 공개 저장소형 구조로 개편
- 저장소 문서 링크를 원격에서 읽기 좋은 상대경로 기준으로 정리
- 멀티플랫폼 릴리즈 구조, OS별 latest 라우트, 원격 Releases 연계 구조로 확장
- 최신 기준 스크린샷을 원격 저장소에 함께 유지하는 정책 반영
- 클라이언트 실제 구현 스택을 `WinUI 3 계획안`에서 `Avalonia 12 실행안`으로 조정
- 프록시 환경에서 WebSocket URL이 `wss://`로 내려오도록 API forwarded headers 처리 추가
- 배포 문서를 실제 운영 구조 기준 `Caddy + ASP.NET Core API + nginx webapp + SQLite`로 정정
- README를 저장소 공개면 중심 구조로 전면 재구성
- 데스크톱 UI를 모던 화이트/플랫/컴팩트 기준으로 대규모 개편하고 기본 서버 주소를 `https://vstalk.phy.kr`로 조정
- 모바일 웹 UI를 화이트 원톤 메신저 셸로 재설계하고 `전체/안읽음/고정` 필터, 검색, 가입 직후 첫 대화 진입 흐름 추가
- 최신 기준 README 스크린샷 자산을 현재 UI 목업과 모바일 웹 캡처 기준으로 갱신
- 모바일 웹 세션 복구를 refresh token 회전 경쟁에 안전한 구조로 보강하고, 일반 네트워크 오류 시 마지막 정상 화면을 유지하도록 조정
- 모바일 웹 대화 전환 시 초안이 다른 방으로 잠깐 보이는 상태 불일치를 줄이고, 자동 스크롤을 하단 근처 또는 내 전송 직후로 제한
- 모바일 웹 상태 메시지를 온보딩/세션 화면에 맞게 분리하고, JSON이 아닌 오류 응답도 친화적 메시지로 처리
- 모바일 웹 최신 기준 목록/대화 스크린샷 자동 캡처 스크립트 추가
- 모바일 웹 하단 바를 목적지형 `대화/검색/보관/내 공간` 구조로 재편하고, 검색/보관/내 공간을 분리된 표면으로 1차 구현
- 모바일 웹 온보딩 카피, 빈 상태 CTA, 내 공간 액션 배치를 덜 기술적이고 더 사용자 중심으로 정리
- 모바일 웹 최신 기준 스크린샷에 검색 화면을 추가하고, 스크린샷 생성 스크립트 의존성을 재현 가능하게 정리
- README, PROJECT_STATUS, 문서 인덱스에 강화된 사용자 리뷰 프레임 링크와 문서 규모를 반영
- 업무/일상 UX 확장 문서를 `118개` 규모의 마스터 세트로 재구성하고, 모바일 웹 실사용 리뷰·저피로 UI 규칙·정보구조·adoption/support 문서를 추가
- README 상단을 `신뢰/상태/진입 경로` 중심으로 재구성하고, 커뮤니티·메인테이너·릴리즈 문서로 공개면을 확장
- Issue / PR 템플릿에 플랫폼, 문서 정합성, UX 리뷰 흐름을 더 명시적으로 반영
- 사용자 관점 리뷰 체계를 `119개` 문서 규모로 확장하고, 다음 단계 플랫폼별/실패유형별 QA 분리 제안서를 추가
- 문서 체계를 루트 120개 + 아틀라스 253개, 총 373개 문서 규모로 확장하고, 실제 모바일 웹 비판 리뷰와 세부 QA 아틀라스를 연결
- 모바일 웹 버전을 `web-0.1.0-alpha.2`로 올리고, 첫 대화방 empty state를 행동 패널로 재설계
- 모바일 웹 검색을 대화/최근 메시지 기반 재발견 표면으로 확장하고 결과를 메시지/대화 섹션으로 분리
- 모바일 웹 보관함을 `답장 필요 / 중요 대화 / 최근 다시 열기` 허브로 재구성
- 세션 신뢰 카피를 현재 화면 유지 중심으로 조정하고, reconnect 후 최신 메시지 재동기화와 초기 WebSocket 재연결을 보강
- 최신 모바일 웹 스크린샷 세트에 `보관함` 화면을 추가하고 캡처 스크립트를 확장
- README 상단을 즉시 체험형 CTA, 공개 계약, 평가 경로, FAQ/Showcase 중심으로 재구성하고 이슈 진입 링크도 함께 정리
- 저장소 전략 기준을 `오픈소스 코어 + 공식 플랫폼/관리형 운영 + 공공/기관 대응 가능성`으로 명문화하고 공개 문서에 반영
- 범용성, 업무형 간편성, 멀티플랫폼, 셀프호스팅/내부망, 보안/운영 투명성, 커뮤니티 기반 개선 구조를 핵심 차별점으로 공개면과 전략 문서에 고정
- 공개 저장소 표면에 메인테이너 실명, 활동명, GitHub 계정, 운영사 `PHYSIA`, 문의 채널을 일관된 기준으로 반영
- 기본 JWT 서명키 거부, 세션 재검증, WebSocket 전용 티켓, 인증 no-store, 기본 rate limiting, 보수적 초대코드 시드 정책으로 기본 보안선을 강화
