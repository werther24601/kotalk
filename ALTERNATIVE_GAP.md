# Why Existing Alternatives Still Leave A Gap

KoTalk가 말하는 문제의식은 단순히 “카카오톡 말고 다른 앱도 있다”는 수준이 아닙니다. 실제로 국내에서 대안으로 거론되는 서비스는 여럿 있습니다. 다만 현재 공개적으로 확인되는 자료를 기준으로 보면, 아래 축을 **한 번에** 충족하는 사례는 아직 보이지 않습니다.

- 한국어 사용자 습관과 국내 업무형 소통에 맞는 제품 표면
- 기관·기업이 내부망 또는 자체 인프라에 올릴 수 있는 배포 선택권
- 특정 사업자에 묶이지 않는 탈중앙화 또는 벤더 락인 완화 방향
- 개인보안과 프라이버시를 전면에 둔 기술적 설계
- 오픈소스, 커뮤니티 기여, 운영 설명 책임, 공개 거버넌스 같은 투명성

이 문서의 요지는 특정 대안을 폄하하는 것이 아닙니다. 오히려 각 대안은 분명한 장점이 있습니다. 문제는 **현재 국내에서 실제로 이동 후보로 거론되는 대안들**과 **보안·자체구축 관점에서 이상형으로 자주 언급되는 대안들**이 서로 다른 축을 잘 풀고 있다는 점입니다. KoTalk는 바로 그 끊어진 축들을 하나의 제품 표면으로 다시 묶으려 합니다.

## What The Publicly Discussed Alternatives Prove

2025년 가을 국내 기사 기준으로, 실제 “카카오톡에서 옮겨갈 수 있는 앱”으로 대중에게 가장 먼저 거론된 축은 LINE, NateOn, WhatsApp, Telegram 같은 소비자용 메신저였습니다. 즉, 시장은 이미 “대안 앱을 찾는 움직임” 자체는 보여 주었습니다.[^ytn-alt]

Telegram은 이 이동 후보 가운데 특히 흥미로운 사례입니다. Telegram 공식 자료는 앱 소스 공개와 reproducible builds를 전면에 둡니다.[^telegram-apps] 동시에 같은 공식 FAQ는 Telegram을 cloud-based messenger로 설명하고, 기술 FAQ에서는 cloud chats의 기본 구조가 client-server encryption이며 end-to-end encryption은 Secret Chats에 해당한다고 구분합니다.[^telegram-cloud][^telegram-tech] 즉, Telegram은 검증 가능성과 사용성 일부에서는 강점을 보이지만, 그것만으로 `자체 구축 + 탈중앙화 + 조직 통제권`까지 해결되지는 않습니다.

동시에 개인정보, 개인보안, 코드 검증 가능성 같은 축에서는 Signal이 꾸준히 강한 참고점으로 언급됩니다. Signal은 공식 지원 문서에서 기본 E2EE와 클라이언트·서버 소스 공개를 분명히 밝힙니다.[^signal-trust] 다만 같은 공식 문서에서 기존 전화번호와 SMS 또는 통화 기반 등록을 요구한다는 점도 분명히 설명합니다.[^signal-phone]

반면 자체 구축, 탈중앙화, 개방형 거버넌스, 조직 통제권 같은 축에서는 Matrix가 가장 강한 기준점입니다. Matrix는 공식 소개에서 자신을 “decentralised, secure communications”를 위한 오픈 프로토콜로 규정하고, 재단의 공개 거버넌스 모델을 전면에 둡니다.[^matrix-about] 또한 공식 호스팅 문서는 자체 호스팅, 조직 데이터 통제, 온프레미스 지원을 명시합니다.[^matrix-hosting]

즉, 현재 공개적으로 보이는 시장은 대략 이렇게 나뉩니다.

- **이동이 쉬운 소비자용 대안**은 존재한다.
- **개인보안과 코드 검증성**을 강하게 보여 주는 대안도 존재한다.
- **자체 구축, 탈중앙화, 공개 거버넌스**를 강하게 보여 주는 대안도 존재한다.

하지만 이 세 묶음이 **한국어 중심의 실제 대중 메신저 표면**에서 한 번에 붙어 있지는 않습니다.

## The Gap KoTalk Is Explicitly Targeting

KoTalk가 강조하는 차별점은 “조금 더 예쁜 메신저”가 아닙니다. 아래의 조합을 하나의 제품 경험으로 붙이는 것입니다.

| Axis | What exists now | Why KoTalk still sees a gap |
|---|---|---|
| 대중적 이동 가능성 | 국내 기사에서 실제로 라인, 네이트온, 왓츠앱, 텔레그램 같은 대안 이동 흐름이 관측됩니다.[^ytn-alt] | 실제 이동 후보로 거론되는 앱들이 곧바로 `자체 구축 + 공개 거버넌스 + 소스 투명성`까지 함께 제공하는 것은 아닙니다. |
| 공개 검증성과 사용성의 절충 | Telegram은 공식적으로 앱 소스 공개와 reproducible builds를 제공하고, 대중적 이동 후보로도 반복 언급됩니다.[^telegram-apps][^ytn-alt] | 그러나 공식 설명 자체가 cloud-based 구조와 cloud chats/secret chats 분리를 전제로 하므로, 기관형 자체구축·탈중앙화 문제까지 한 번에 풀어 주는 해답은 아닙니다.[^telegram-cloud][^telegram-tech] |
| 개인보안 | Signal은 기본 E2EE, 소스 공개, 안전번호 검증을 분명히 제시합니다.[^signal-trust] | 그러나 한국어 중심 대중 메신저 전환 표면, 기관용 내부망 배치, 국내 업무 흐름 친화 UX까지 한 번에 해결한다고 보긴 어렵습니다. |
| 조직 통제권 | Matrix는 탈중앙화, 자체 호스팅, 온프레미스, 공개 거버넌스를 강하게 제공합니다.[^matrix-about][^matrix-hosting] | 하지만 현재 국내 대중이 “카톡 대안”으로 바로 이동하는 소비자용 표면과는 거리가 있습니다. |
| 투명성 | Signal, Matrix는 각각 소스 공개나 공개 거버넌스 측면에서 강점이 있습니다.[^signal-trust][^matrix-about] | 반대로 국내에서 쉽게 떠올리는 대중 메신저 대안은 이 투명성 축을 전면 가치로 제시하지 않는 경우가 많습니다. |

## The Claim, Stated Carefully

KoTalk는 이 점을 분명히 말합니다.

> **현재 국내에서 대안으로 공개적으로 거론되는 서비스들과, 보안·자체구축 관점에서 이상형으로 참조되는 서비스들을 함께 놓고 보더라도, `한국어 중심 UX + 개인보안 + 내부망/자체구축 + 탈중앙화/락인 완화 + 커뮤니티 기반 투명성`을 한 번에 충족하는 사례는 공개 자료 기준으로 확인되지 않았습니다.**

이 문장은 시장 전체를 영원히 단정하는 선언이 아닙니다. 다만 **현재 공개적으로 확인 가능한 자료와 국내 대중 이동 담론**을 놓고 보면, KoTalk가 겨냥하는 결합점이 여전히 비어 있다는 뜻입니다.

## Why This Matters

이 차이는 단순한 포지셔닝 문구가 아니라 제품 방향에 직접 연결됩니다.

- 사내/기관 보안: 외부 SaaS에만 의존하지 않고 내부망·자체 인프라 선택권을 열어야 합니다.
- 개인보안: 대화 상대, 장치, 세션, 전송 실패, 재연결, 검증 가능한 암호화 흐름을 기본값으로 다뤄야 합니다.
- 투명성: 기능보다 먼저 정책·상태·데이터 경계를 설명할 수 있어야 합니다.
- 커뮤니티성: 폐쇄적 단일 결정 구조보다 공개 저장소, 이슈, 릴리즈, 문서, 피드백 반영 기록이 남아야 합니다.

KoTalk는 이 네 가지를 보조 가치가 아니라 **핵심 가치**로 둡니다.

## Read Next

- 배경 맥락: [BACKGROUND.md](BACKGROUND.md)
- 현재 상태: [PROJECT_STATUS.md](PROJECT_STATUS.md)
- 신뢰 표면: [TRUST_CENTER.md](TRUST_CENTER.md)
- 배포 모드: [DEPLOYMENT_MODES.md](DEPLOYMENT_MODES.md)
- 마스터 플랜: [문서/README.md](문서/README.md)

[^ytn-alt]: YTN, `["나 간다?"...급기야 카톡 탈출 움직임](https://www.ytn.co.kr/_ln/0103_202509301503327210_001)` (2025-09-30). 기사에는 LINE, NateOn, WhatsApp, Telegram 등이 실제 대체 메신저 후보로 급부상한 흐름이 정리돼 있습니다.
[^telegram-apps]: Telegram, [`Telegram Applications`](https://telegram.org/apps?setln=be). Telegram은 앱 소스 공개와 reproducible builds를 공식적으로 설명합니다.
[^telegram-cloud]: Telegram, [`Telegram Messenger`](https://telegram.org/help/settings). Telegram은 자신을 cloud-based mobile and desktop messaging app으로 설명합니다.
[^telegram-tech]: Telegram, [`FAQ for the Technically Inclined`](https://core.telegram.org/techfaq). Telegram은 cloud chats의 client-server encryption과 Secret Chats의 end-to-end encryption을 구분해 설명합니다.
[^signal-trust]: Signal Support, [`Is it private? Can I trust it?`](https://support.signal.org/hc/en-us/articles/360007320391-Is-it-private-Can-I-trust-it). Signal은 기본 E2EE와 클라이언트/서버 소스 공개를 공식적으로 설명합니다.
[^signal-phone]: Signal Support, [`Register a phone number`](https://support.signal.org/hc/en-us/articles/360007318691-Register-a-phone-number). Signal은 기존 전화번호와 SMS/통화 기반 등록을 요구한다고 설명합니다.
[^matrix-about]: Matrix.org Foundation, [`About Matrix`](https://matrix.org/foundation/about/). Matrix는 자신을 분산형 보안 통신 오픈 프로토콜로 소개하고, 공개 거버넌스를 명시합니다.
[^matrix-hosting]: Matrix.org, [`Hosting`](https://matrix.org/ecosystem/hosting/). Matrix 공식 문서는 자체 호스팅, 조직 데이터 통제, 온프레미스/운영 지원 사례를 제시합니다.
