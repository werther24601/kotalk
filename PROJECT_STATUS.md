# Project Status

마지막 검증일: `2026-04-16`

## Status Dashboard

| Signal | Current read |
|---|---|
| Public brand | `KoTalk` |
| Stage | `Alpha` |
| Most usable surface | Mobile web live + Windows build |
| Biggest current gap | Android 실빌드와 데스크톱 멀티윈도우 완성도 |
| Signup direction | 공개형 1회성 인증 중심으로 재설계 중 |
| Tone of this repo | 현재 동작 범위와 남은 갭을 함께 적는 제품형 저장소 |

## What Exists Right Now

KoTalk는 아직 모든 플랫폼이 완성된 상태는 아니지만, “문서만 있는 프로젝트” 단계는 이미 지났습니다. 현재 저장소 기준으로 아래 항목을 실제로 확인할 수 있습니다.

- Windows 데스크톱 클라이언트 빌드
- 모바일 웹 실서비스 채널
- 기본 인증, 최근 대화, 메시지 전송, 읽기 커서, 세션 복구 루프
- 최신 기준 스크린샷 세트
- 릴리즈 경로와 다운로드 경로 문서

## Channel Status

| Channel | Surface | Status | Notes |
|---|---|---|---|
| Windows desktop | 저장소 빌드 / 릴리즈 산출물 | Buildable | 핵심 메시징 루프 검증 가능 |
| Mobile web | [vstalk.phy.kr](https://vstalk.phy.kr) | Live | 가입, 대화, 검색, 보관 1차 흐름 제공 |
| Android | 저장소 릴리즈 예정 | In progress | 문서와 배포 구조 우선 정리 중 |
| Official mirror | [download-vstalk.phy.kr](https://download-vstalk.phy.kr) | Live | Windows latest와 version manifest를 HTTPS로 제공 |

## Verified Now

현재 기준으로 확인된 사실만 적습니다.

- Windows 클라이언트는 저장소 기준으로 빌드 가능한 상태입니다.
- 모바일 웹은 [vstalk.phy.kr](https://vstalk.phy.kr)에서 공개 중입니다.
- 기본 메시징 루프와 세션 복구 흐름은 구현돼 있습니다.
- 검색, 보관, 빈 상태 UX는 1차 개편이 반영돼 있습니다.
- 최신 스크린샷은 저장소에 함께 보관됩니다.

## Visual Proof

| Surface | Proof |
|---|---|
| Desktop shell | [hero-shell.png](docs/assets/latest/hero-shell.png) |
| Desktop onboarding | [onboarding.png](docs/assets/latest/onboarding.png) |
| Desktop conversation | [conversation.png](docs/assets/latest/conversation.png) |
| Mobile web onboarding | [vstalk-web-onboarding.png](docs/assets/latest/vstalk-web-onboarding.png) |
| Mobile web inbox | [vstalk-web-list.png](docs/assets/latest/vstalk-web-list.png) |
| Mobile web search | [vstalk-web-search.png](docs/assets/latest/vstalk-web-search.png) |
| Mobile web saved | [vstalk-web-saved.png](docs/assets/latest/vstalk-web-saved.png) |
| Mobile web chat | [vstalk-web-chat.png](docs/assets/latest/vstalk-web-chat.png) |

## Product Direction That Is Already Visible

현재 화면만 봐도 읽히는 제품 방향은 아래와 같습니다.

- 메시징을 중심에 두고, 피드형 잡음을 덜어내려는 구조
- 텍스트를 장황하게 읽게 하기보다 구조와 위치로 이해시키는 UI
- 한국어 데스크톱 사용성, 특히 반복적인 읽기와 답장 흐름을 중시하는 설계
- 단순한 “보여주기용 스크린샷”이 아니라 실제 릴리즈와 상태 문서에 연결된 표면

## In Progress

- Android 첫 실사용 빌드
- 릴리즈 페이지와 미러 간 latest 라우트 통합
- 검색 범위 확장
- 파일 전송
- 데스크톱 멀티 윈도우 생산성 강화

## Current Limits

아직 부족한 부분도 그대로 남깁니다.

- Android 실사용 빌드는 아직 제공되지 않습니다.
- 파일 전송은 미구현입니다.
- 검색은 전역 파일/링크/사람 범위까지 확장되지 않았습니다.
- 공식 다운로드 미러는 현재 Windows latest와 version manifest 기준으로 동작합니다.
- 데스크톱 멀티 윈도우는 방향은 잡혀 있지만, 실제 생산성 흐름은 더 다듬어야 합니다.

## Download And Release Paths

| Path | Purpose |
|---|---|
| [download-vstalk.phy.kr](https://download-vstalk.phy.kr) | 공식 다운로드 미러 주소 |
| [download-vstalk.phy.kr/windows/latest](https://download-vstalk.phy.kr/windows/latest) | Windows latest |
| [download-vstalk.phy.kr/android/latest](https://download-vstalk.phy.kr/android/latest) | Android latest |
| [download-vstalk.phy.kr/latest/version.json](https://download-vstalk.phy.kr/latest/version.json) | 버전 메타데이터 |
| [physia.kr/open-source/projects/public/kotalk](https://physia.kr/open-source/projects/public/kotalk) | 제2 공개 레포 |
| [Forge releases](https://git.physia.kr/ian/vs-messanger/releases) | 저장소 릴리즈 채널 |
| [GitHub releases](https://github.com/werther24601/kotalk/releases) | 공개 릴리즈 채널 |

## Why This Repo May Feel Denser Again

최근 공개 표면은 리스크를 줄이려는 과정에서 지나치게 건조해졌습니다. 현재는 다시 아래 균형을 맞추는 방향으로 조정 중입니다.

- 화면과 스크린샷은 충분히 보여 주되, 과장된 약속은 줄이기
- 제품 배경과 문제의식은 다시 설명하되, 감정적인 공격은 피하기
- 상태 문서는 짧게 유지하되, 이 저장소가 왜 존재하는지는 읽히게 만들기

배경 문서는 [BACKGROUND.md](BACKGROUND.md), 더 긴 맥락은 [문서/14-project-background-and-market-context.md](문서/14-project-background-and-market-context.md)에서 확인할 수 있습니다.

## Review Focus

- 사용자 관점 리뷰: [문서/31-user-review-log-and-experience-scorecard.md](문서/31-user-review-log-and-experience-scorecard.md)
- 현재 모바일 웹 리뷰: [문서/89-current-product-mobile-web-review-2026-04.md](문서/89-current-product-mobile-web-review-2026-04.md)
- 라이브 우선순위: [문서/35-live-user-review-and-priority-backlog.md](문서/35-live-user-review-and-priority-backlog.md)
- 가입/온보딩 정책: [문서/10-signup-onboarding-and-auth-policy.md](문서/10-signup-onboarding-and-auth-policy.md)
- 신뢰와 보안 표면: [TRUST_CENTER.md](TRUST_CENTER.md), [SECURITY.md](SECURITY.md)
