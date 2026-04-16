# Releasing

KoTalk의 릴리즈는 단순한 파일 업로드가 아니라, 산출물과 공개 문서가 같은 상태를 가리키도록 맞추는 작업입니다.

## Release Surfaces

- 공식 다운로드 미러: [download-vstalk.phy.kr](https://download-vstalk.phy.kr)
- Windows latest: [download-vstalk.phy.kr/windows/latest](https://download-vstalk.phy.kr/windows/latest)
- Android latest: [download-vstalk.phy.kr/android/latest](https://download-vstalk.phy.kr/android/latest)
- 버전 메타데이터: [download-vstalk.phy.kr/latest/version.json](https://download-vstalk.phy.kr/latest/version.json)
- 제2 공개 레포: [physia.kr/open-source/projects/public/kotalk](https://physia.kr/open-source/projects/public/kotalk)
- Forge releases: [git.physia.kr/ian/vs-messanger/releases](https://git.physia.kr/ian/vs-messanger/releases)
- GitHub releases: [github.com/werther24601/kotalk/releases](https://github.com/werther24601/kotalk/releases)

## Current Note

2026-04-16 기준 [download-vstalk.phy.kr](https://download-vstalk.phy.kr)의 DNS/HTTPS 정합성은 재점검이 필요합니다.
그래서 현재는 저장소 릴리즈 경로를 함께 유지하는 것을 원칙으로 둡니다.

## Minimum Release Contract

1. 실제로 실행 가능한 산출물이 있어야 합니다.
2. [README.md](README.md)와 [PROJECT_STATUS.md](PROJECT_STATUS.md)가 같은 상태를 가리켜야 합니다.
3. [CHANGELOG.md](CHANGELOG.md)에 의미 있는 변경이 기록돼야 합니다.
4. 최신 스크린샷이 현재 UI를 대표해야 합니다.
5. 다운로드 경로와 릴리즈 링크가 함께 갱신돼야 합니다.

## Platform Policy

- Windows: 빌드 산출물, 스크린샷, 체크섬을 함께 남깁니다.
- Mobile web: 라이브 반영이 있으면 스크린샷과 상태 문서를 함께 갱신합니다.
- Android: APK 공개 시 공식 미러와 저장소 릴리즈를 함께 맞춥니다.

## Related Docs

- 배포 골격: [deploy/README.md](deploy/README.md)
- 릴리즈 메타데이터: [release-assets/README.md](release-assets/README.md)
- 상태표: [PROJECT_STATUS.md](PROJECT_STATUS.md)
