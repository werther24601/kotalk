# Review Surface Expansion And Critical QA Proposal

## 목적

이 문서는 현재 `KoTalk` 산출물 기준에서 사용자 관점 리뷰와 비판적 견해를 더 촘촘하게 남기기 위해, 앞으로 추가해야 할 리뷰 문서와 QA 문서 범주를 제안한다.

기존 문서가 부족해서라기보다, 현재는 `총평`, `통합 감사`, `플랫폼 리뷰`에 내용이 너무 많이 모여 있다. 그 결과 아래 문제가 남아 있다.

- 모바일 웹과 Windows 준비도 리뷰가 같은 밀도로 다뤄지지 않는다.
- 가입, 검색, 세션, 빈 상태가 각각 독립된 실패 분석 문서로 쪼개져 있지 않다.
- 업무형/친근형 흐름 리뷰는 있으나, 각 흐름의 실패 재현 체크가 아직 약하다.
- QA가 `어디를 봐야 하는가`는 알려 주지만, `어떤 실패를 우선 부숴야 하는가`는 더 선명해야 한다.

이 문서는 Product, UX Research, QA, Mobile Web Reviewer, Windows Readiness Reviewer, Workflow Critic 관점을 합쳐 작성한다.

## 먼저 고정할 비판 포인트

새 문서를 쓰기 전에, 현재 산출물 기준에서 가장 먼저 더 날카롭게 봐야 할 비판 포인트를 우선순위로 고정한다.

### P0

- 모바일 웹은 가입은 빠르지만, 가입 직후 `앱 사용`보다 `도구 조작`처럼 느껴질 위험이 아직 있다.
- 검색은 진입 구조가 좋아졌지만, 업무형 사용자가 `메시지`, `자료`, `사람`, `방금 하던 일`을 다시 찾는 수준에는 아직 못 미친다.
- 세션 복구는 기술적으로 전보다 안정적이지만, 사용자 체감은 여전히 `잠깐 흔들리면 나가떨어질 수도 있겠다`에 가깝다.
- 빈 상태는 예전보다 부드러워졌지만, 여전히 `막힘 해소`보다 `설명`에 머물 때가 있다.
- 업무형 사용자는 `답장 필요`, `나중에 처리`, `공유 자료 재발견`, `최근 작업 복귀`가 약해 기존 메신저 대비 전환 근거가 약하다.

### P1

- Windows 채널은 공개 스크린샷과 문서 기준으로 방향은 좋지만, `실제로 계속 켜 두고 쓰는 생산성 도구`라는 증거가 더 필요하다.
- 친근형 흐름은 차갑지 않게 개선됐지만, 여전히 `가볍게 열고 툭 보내는 생활형 리듬`보다는 `정돈된 알파`에 가깝다.
- 검색 실패, 초대코드 실패, 일시 서버 문제, 세션 만료가 사용자의 머릿속에서 충분히 다른 실패로 분기되지 않는다.
- 공개 저장소의 문서 밀도는 높지만, 리뷰 문서가 많아질수록 `무엇이 실제 현 산출물 평인지`와 `무엇이 미래 설계인지`를 더 분리해야 한다.

### P2

- Windows와 모바일 웹의 체감 차이를 비교하는 병렬 리뷰가 없다.
- 검색, 세션, 빈 상태, 업무형 흐름이 버전별로 어떻게 개선됐는지 추적하는 시계열 리뷰 문서가 부족하다.
- 친근형 흐름에서 사진, 링크, 셀프 메시지, 짧은 반응 같은 생활형 장치에 대한 비판적 리뷰가 더 필요하다.

## 추가해야 할 리뷰/QA 문서 24개

아래 24개는 `새로 있으면 좋은 문서`가 아니라, 현재 산출물의 약점을 더 정확히 드러내기 위해 필요한 문서다.

### A. 모바일 웹 실사용 리뷰 심화 6개

#### 1. `113-mobile-web-first-3-minutes-review.md`

- 목적: 첫 방문부터 가입 직후 첫 대화까지 3분 안에 생기는 혼선을 기록
- 핵심 질문: 사용자가 `설명` 없이 첫 대화에 도달하는가
- 우선순위: `P0`

#### 2. `114-mobile-web-navigation-clarity-review.md`

- 목적: 하단 목적지, 상단 필터, 뒤로 가기, 검색 진입의 역할 충돌 여부 점검
- 핵심 질문: 사용자가 지금 `이동 중인지`, `같은 화면 안에서 거르는 중인지` 즉시 설명할 수 있는가
- 우선순위: `P0`

#### 3. `115-mobile-web-empty-state-cta-review.md`

- 목적: 대화 없음, 검색 무결과, 안읽음 없음, 고정 없음, 보관함 비어 있음에서 다음 행동이 명확한지 검증
- 핵심 질문: 빈 상태가 `막힘`으로 읽히는가, `행동 유도`로 읽히는가
- 우선순위: `P0`

#### 4. `116-mobile-web-thumb-zone-and-reachability-qa.md`

- 목적: 한 손 탐색 기준에서 엄지 이동량과 오작동 가능성 검토
- 핵심 질문: 핵심 조작이 화면 하단 60% 안에서 끝나는가
- 우선순위: `P1`

#### 5. `117-mobile-web-status-copy-and-trust-review.md`

- 목적: 연결 중, 복구 중, 실패, 재시도 상태 문구가 안심감을 주는지 검토
- 핵심 질문: 상태 이름보다 `지금 계속 써도 되는가`가 더 잘 전달되는가
- 우선순위: `P0`

#### 6. `118-mobile-web-repeat-use-fatigue-review.md`

- 목적: 1회 체험이 아니라 3일 반복 사용 기준의 피로도 기록
- 핵심 질문: 다시 켰을 때도 여전히 가볍고 빠르게 느껴지는가
- 우선순위: `P1`

### B. Windows 준비도와 데스크톱 체감 리뷰 5개

#### 7. `119-windows-first-usable-alpha-readiness-review.md`

- 목적: 현재 Windows 채널이 `빌드 가능`을 넘어 `계속 사용 가능한가`를 판단
- 핵심 질문: 문서와 스크린샷이 아니라 실제 산출물 기준으로 생산성 우위가 보이는가
- 우선순위: `P0`

#### 8. `120-windows-multiwindow-and-resize-qa.md`

- 목적: 창 크기 변화, 2열/3열, 팝아웃 가능성, 좁은 폭에서의 정보 위계 검증
- 핵심 질문: 데스크톱다운 장점이 실제로 체감되는가
- 우선순위: `P1`

#### 9. `121-windows-search-and-command-surface-review.md`

- 목적: 전역 검색, 최근 대화 복귀, 키보드 중심 이동의 준비도 점검
- 핵심 질문: 마우스보다 빠른 업무 루프가 가능한가
- 우선순위: `P1`

#### 10. `122-windows-session-draft-and-recovery-qa.md`

- 목적: 세션 재개, 초안 보존, 창 전환 후 복귀 안전성 검증
- 핵심 질문: Windows 채널이 모바일보다 더 안심되는가
- 우선순위: `P0`

#### 11. `123-windows-open-all-day-productivity-review.md`

- 목적: 하루 종일 켜 두는 업무 메신저 기준으로 알림, 복귀, 방 전환 피로를 평가
- 핵심 질문: `예쁜 데스크톱 앱`이 아니라 `계속 켜 두는 도구`가 되었는가
- 우선순위: `P1`

### C. 가입, 검색, 세션, 빈 상태 실패 분석 7개

#### 12. `124-signup-failure-taxonomy-and-review.md`

- 목적: 초대코드 오류, 이름 입력 문제, 서버 지연, 네트워크 문제를 사용자 관점에서 분리 기록
- 핵심 질문: 실패 원인을 사용자가 3초 안에 구분할 수 있는가
- 우선순위: `P0`

#### 13. `125-signup-copy-trust-and-dropoff-review.md`

- 목적: 온보딩 카피와 CTA가 기술적 불안을 주는 지점 분석
- 핵심 질문: 첫 화면이 여전히 개발자용 냄새를 풍기지 않는가
- 우선순위: `P1`

#### 14. `126-search-zero-results-and-recovery-qa.md`

- 목적: 검색 결과 없음, 잘못된 검색어, 너무 넓은 검색에서 회복 경로 검증
- 핵심 질문: 무결과가 `실패`가 아니라 `다음 탐색`으로 이어지는가
- 우선순위: `P0`

#### 15. `127-search-depth-and-knowledge-retrieval-review.md`

- 목적: 대화, 메시지, 파일, 링크, 사람 재발견 관점에서 검색 깊이 평가
- 핵심 질문: 업무형 사용자가 기억 대신 검색으로 문제를 해결하는가
- 우선순위: `P0`

#### 16. `128-session-recovery-failure-matrix.md`

- 목적: refresh 실패, 네트워크 순간 끊김, 토큰 만료, 서버 5xx를 사용자 체감 기준으로 분리
- 핵심 질문: 서로 다른 실패가 모두 `로그아웃당함`처럼 읽히지 않는가
- 우선순위: `P0`

#### 17. `129-last-good-state-and-reentry-review.md`

- 목적: 마지막 정상 화면 유지 정책이 실제로 안심감을 주는지 점검
- 핵심 질문: 복귀 중에도 사용자가 현재 맥락을 잃지 않는가
- 우선순위: `P0`

#### 18. `130-empty-state-by-surface-qa.md`

- 목적: 목록, 검색, 보관, 채팅, 내 공간 각 표면의 빈 상태를 별도 QA 시나리오로 검증
- 핵심 질문: 각 빈 상태가 하나의 분명한 다음 행동을 주는가
- 우선순위: `P0`

### D. 업무형 흐름과 친근형 흐름 리뷰 4개

#### 19. `131-workflow-triage-and-reply-later-review.md`

- 목적: 업무형 사용자의 `안읽음 처리`, `나중에 답장`, `최근 작업 복귀` 흐름 평가
- 핵심 질문: 바쁜 사용자가 메신저를 `처리 도구`처럼 쓸 수 있는가
- 우선순위: `P0`

#### 20. `132-workflow-search-share-and-handoff-review.md`

- 목적: 파일/링크/결정사항/공유자료를 다시 찾아 전달하는 흐름 검토
- 핵심 질문: 업무형 소통이 대화창 스크롤에 묻히지 않는가
- 우선순위: `P1`

#### 21. `133-friendly-flow-lightness-and-warmth-review.md`

- 목적: 친근형 사용자가 앱을 `부담 없이 열고 보내는지` 감정 리듬 평가
- 핵심 질문: 지나치게 차갑거나 도구적으로 느껴지지 않는가
- 우선순위: `P1`

#### 22. `134-friendly-flow-photo-link-self-message-qa.md`

- 목적: 사진, 링크, 셀프 메시지, 짧은 응답, 가벼운 재진입 패턴 검증
- 핵심 질문: 친근한 소통이 업무형 설계에 눌려 숨지 않는가
- 우선순위: `P2`

### E. 교차 QA와 버전 추적 문서 2개

#### 23. `135-cross-platform-critique-mobile-vs-windows.md`

- 목적: 같은 사용자가 모바일 웹과 Windows를 오가며 느끼는 차이를 비교
- 핵심 질문: 채널별로 장점과 불안이 어떻게 달라지는가
- 우선순위: `P1`

#### 24. `136-versioned-review-diff-and-regression-log.md`

- 목적: 버전별로 좋아진 점, 그대로인 약점, 새로 생긴 퇴행을 기록
- 핵심 질문: 다음 버전이 실제로 더 좋아졌는지 증거가 남는가
- 우선순위: `P1`

## 실제로 먼저 써야 할 순서

24개를 한 번에 늘리는 대신, 아래 순서로 자르는 것이 맞다.

### 1차 작성 묶음

- `113-mobile-web-first-3-minutes-review.md`
- `114-mobile-web-navigation-clarity-review.md`
- `117-mobile-web-status-copy-and-trust-review.md`
- `124-signup-failure-taxonomy-and-review.md`
- `126-search-zero-results-and-recovery-qa.md`
- `128-session-recovery-failure-matrix.md`
- `130-empty-state-by-surface-qa.md`
- `131-workflow-triage-and-reply-later-review.md`

이 8개는 현재 산출물 약점을 가장 직접적으로 드러낸다.

### 2차 작성 묶음

- `119-windows-first-usable-alpha-readiness-review.md`
- `122-windows-session-draft-and-recovery-qa.md`
- `127-search-depth-and-knowledge-retrieval-review.md`
- `129-last-good-state-and-reentry-review.md`
- `132-workflow-search-share-and-handoff-review.md`
- `135-cross-platform-critique-mobile-vs-windows.md`

이 6개는 `왜 기존 메신저보다 더 편한가`를 입증하는 데 필요하다.

### 3차 작성 묶음

- `116-mobile-web-thumb-zone-and-reachability-qa.md`
- `118-mobile-web-repeat-use-fatigue-review.md`
- `120-windows-multiwindow-and-resize-qa.md`
- `121-windows-search-and-command-surface-review.md`
- `123-windows-open-all-day-productivity-review.md`
- `125-signup-copy-trust-and-dropoff-review.md`
- `133-friendly-flow-lightness-and-warmth-review.md`
- `134-friendly-flow-photo-link-self-message-qa.md`
- `136-versioned-review-diff-and-regression-log.md`

이 9개는 제품이 알파를 넘어서려면 반드시 필요하지만, 당장 가장 앞의 불안 요소를 정리한 뒤 들어가는 편이 낫다.

## 문서 작성 원칙

새 리뷰 문서는 모두 아래 원칙을 따라야 한다.

- 칭찬보다 실패 신호를 먼저 쓴다.
- `좋다/나쁘다`보다 `어디서 왜 멈췄는가`를 쓴다.
- 실제 화면, 실제 빌드, 실제 링크, 실제 버전을 기준으로 쓴다.
- 설계 의도와 현재 산출물 평을 같은 문단에 섞지 않는다.
- 마지막에는 반드시 `즉시 고칠 것`, `보류할 것`, `다음 버전에서 다시 볼 것`을 남긴다.

## 현재 기준 한 줄 결론

지금 필요한 것은 더 많은 총평이 아니라, `가입`, `검색`, `세션`, `빈 상태`, `업무형 복귀`, `친근형 리듬`, `Windows 준비도`를 각각 따로 찢어서 냉정하게 남기는 리뷰 문서들이다.
