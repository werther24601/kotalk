# FAQ

KoTalk를 처음 보는 사람을 위한 짧은 안내입니다.

## KoTalk는 무엇인가요

KoTalk는 한국어 중심의 메시징 경험을 다시 설계하는 오픈소스 프로젝트입니다. 현재는 Windows 데스크톱과 모바일 웹을 우선 채널로 두고, Android를 병렬 확장하고 있습니다.

## 왜 Windows를 먼저 만드나요

이 프로젝트의 강점이 큰 화면에서 드러나는 검색, 복귀, 다중 창, 후속조치 흐름에 있기 때문입니다. 모바일보다 데스크톱에서 생산성 차이가 더 분명하게 드러난다고 판단했습니다.

## 모바일 웹은 어디서 볼 수 있나요

[vstalk.phy.kr](https://vstalk.phy.kr)에서 현재 공개 중인 모바일 웹 진입점을 확인할 수 있습니다.

## 특정 메신저를 그대로 복제하려는 프로젝트인가요

아닙니다. 익숙한 구조는 참고하지만, 목표는 더 낮은 피로도와 더 짧은 복귀 흐름입니다. 복제보다 정제에 가깝습니다.

## 이미 대안 메신저가 많은데 왜 또 필요한가요

국내에서 실제로 이동 후보로 거론되는 메신저는 이미 있습니다. 다만 공개 자료를 기준으로 보면, `한국어 중심 UX`, `개인보안`, `기관·기업의 내부망 또는 자체 구축`, `탈중앙화 또는 락인 완화`, `오픈소스와 운영 투명성`을 한 번에 묶어 보여 주는 사례는 아직 뚜렷하지 않습니다. 이 프로젝트는 바로 그 결합점을 목표로 둡니다. 자세한 설명은 [ALTERNATIVE_GAP.md](ALTERNATIVE_GAP.md)에 따로 정리했습니다.

## 지금 실제로 되는 것은 무엇인가요

- Windows 빌드
- 모바일 웹 라이브 진입
- 기본 계정 생성과 세션 유지
- 대화 목록, 메시지 전송, 읽기 반영
- 검색/보관/빈 상태 1차 UX

정확한 범위는 [PROJECT_STATUS.md](PROJECT_STATUS.md)에서 확인할 수 있습니다.

## 가입 방식은 어떻게 가나요

현재 알파 단계에서는 통제된 접근 정책을 쓰고 있지만, 공개 기획 기준은 `초대코드 중심`보다 `이메일 또는 휴대폰 기반 1회성 인증` 쪽으로 이동하고 있습니다. 자세한 배경은 [문서/10-signup-onboarding-and-auth-policy.md](문서/10-signup-onboarding-and-auth-policy.md)에 정리했습니다.

## 다운로드는 어디서 받나요

공식 미러와 저장소 릴리즈를 함께 제공합니다.

- 공식 미러: [download-vstalk.phy.kr](https://download-vstalk.phy.kr)
- 제2 공개 레포: [physia.kr/open-source/projects/public/kotalk](https://physia.kr/open-source/projects/public/kotalk)
- Forge releases: [git.physia.kr/ian/vs-messanger/releases](https://git.physia.kr/ian/vs-messanger/releases)
- GitHub releases: [github.com/werther24601/kotalk/releases](https://github.com/werther24601/kotalk/releases)

현재 미러 정합성 상태는 [PROJECT_STATUS.md](PROJECT_STATUS.md), 릴리즈 규칙은 [RELEASING.md](RELEASING.md)에 기록합니다.

## 공식 서비스와 오픈소스 저장소는 같은가요

같지 않습니다. 저장소는 오픈소스 코어와 공개 문서를 다루고, 운영 서비스는 별도 표면으로 관리합니다. 이 경계는 [BUSINESS_MODEL.md](BUSINESS_MODEL.md)와 [DEPLOYMENT_MODES.md](DEPLOYMENT_MODES.md)에 설명해 두었습니다.

## 라이선스는 무엇인가요

현재 저장소는 [Apache License 2.0](LICENSE)을 사용합니다. 상표는 별도 정책을 따르므로 [TRADEMARKS.md](TRADEMARKS.md)도 함께 봐야 합니다.

## 기여는 어떻게 시작하나요

[CONTRIBUTING.md](CONTRIBUTING.md), [FIRST_CONTRIBUTION.md](FIRST_CONTRIBUTION.md), [COMMUNITY.md](COMMUNITY.md)를 순서대로 보면 가장 빠릅니다.

## 보안 이슈는 어디로 알려야 하나요

공개 이슈보다 먼저 [SECURITY.md](SECURITY.md)에 적힌 비공개 경로를 사용해 주세요.
