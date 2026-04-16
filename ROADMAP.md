# Roadmap

## 현재 상태

- Alpha 가입, 대화 목록, 대화창, 텍스트 전송이 동작하는 첫 사용 가능 프로토타입 확보
- Windows x64 installer / onefile / zip 생성 가능
- Android APK alpha 기준선 생성 가능
- `vstalk.phy.kr` 모바일 웹앱과 API를 VPS에 실제 배포
- 원격 저장소에 최신 기준 스크린샷 포함 시작
- `vstalk.phy.kr` 모바일 웹앱 MVP 빌드 및 same-origin API 검증 완료

## v0.1 Alpha

- [x] 초대코드 기반 가입
- [x] 최근 대화 로드
- [x] 메시지 전송
- [x] 읽기 커서 갱신
- [x] Windows portable zip 생성
- [x] Windows installer / onefile / zip 생성
- [x] 모바일 웹앱 PWA 셸/가입/대화/전송
- [x] VPS 공개 API 상시 구동
- [x] `vstalk.phy.kr` same-origin 웹앱 배포
- [x] Android WebView 셸과 첫 APK 생성
- [ ] 데스크톱 WebSocket 실시간 반영

## v0.2 Collaboration Basics

- [ ] 파일 전송
- [ ] 메시지 검색
- [ ] 고정 / 읽지 않음 / 보관 UX
- [ ] 알림/트레이 고도화
- [ ] 공개 다운로드 채널 개통

## v0.2 Android First-class

- [x] Android 셸/네비게이션 골격
- [x] Android 로그인/대화 목록/대화 진입 MVP 기준선
- [ ] APK 서명 및 산출물 규칙 확정
- [ ] Windows/Android 동시 릴리즈 메타데이터 검증
- [ ] Forge Releases + VPS 미러 동시 게시

## v0.3 Shared Client Expansion

- [ ] Avalonia 기반 공유 클라이언트 구조를 Android/iOS/Linux 관점으로 재정리
- [ ] iOS 배포 채널 준비 문서와 빌드 체인 검증
- [ ] Linux 패키징 기준선 확보
- [ ] Android WebView 셸에서 네이티브 공용 UI 단계로 이행할 범위 정의

## v0.2 Mobile Web Entry

- [x] `vstalk.phy.kr` 모바일 웹 IA와 핵심 사용자 흐름 확정
- [x] 링크 진입 화면과 초간단 가입/로그인 구조 정리
- [x] 최근 대화 목록과 대화 화면 모바일 MVP 구현
- [x] 업무/친근 소통 공존 규칙과 활동 화면 우선순위 문서화
- [x] PWA 도입 전제와 비포함 범위 문서화
- [x] 실제 VPS 배포 및 same-origin API 검증
- [ ] 웹앱 전용 최신 스크린샷 갱신 자동화

## v0.3 Reliability

- [ ] 재연결 UX 고도화
- [ ] 로컬 Draft/캐시 안정화
- [ ] 자동 업데이트 전략 수립
- [ ] 배포 자동화와 릴리즈 검증 강화
- [ ] 최신 기준 스크린샷 갱신 자동 체크리스트 정착

## v1.0 Preview

- [ ] 다중 기기 세션 관리
- [ ] Passkey 또는 더 강한 로그인 흐름
- [ ] 업무/개인 맥락 전환 UX
- [ ] 장기 보관과 검색 품질 개선

## 사업화와 조달 대응 방향

- [ ] `오픈소스 코어 / 공식 플랫폼 / 기관 대응 기능` 경계 문서화
- [ ] 감사 로그, 관리자 정책, 조직 계정 모델의 최소 골격 확보
- [ ] 셀프호스팅 배포 문서와 관리형 플랫폼 운영 문서 분리
- [ ] 접근성, 릴리즈 증빙, 배포 선택권을 공공/기관 대응 기준으로 축적
- [ ] 크라우드 펀딩용 공개 표면과 릴리즈/상태표 정합성 유지

## 보류 또는 실험 항목

- 음성/영상 통화
- 공개 커뮤니티
- 결제/송금
- 피드형 콘텐츠
- 과도한 자동 보조 기능 탑재

## 기여 우선순위

1. Android 최소 런칭 패스 구축
2. 데스크톱 실시간 동기화
3. 파일 전송과 검색
4. Forge Releases와 다운로드 미러 정합성
5. Android 첫 배포 이후 릴리즈 자동화

## 멀티 OS 릴리즈 운영 규칙

- 하나의 태그는 하나의 릴리즈 레코드를 뜻합니다.
- 같은 버전 번호 아래에 Windows와 Android 자산을 함께 게시합니다.
- 원격 저장소 릴리즈 Assets에는 실행 파일과 체크섬만 두고, 최신 스크린샷은 변경 노트 안에서 참조합니다.
- 다운로드 미러와 원격 Releases는 같은 자산 이름과 같은 노트를 기준으로 맞춥니다.
- 모바일 웹은 설치형 산출물 대신 `https://vstalk.phy.kr`를 기준 진입점으로 관리합니다.
