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

2026-04-16 기준 [download-vstalk.phy.kr](https://download-vstalk.phy.kr)는 DNS와 HTTPS가 정상입니다.
현재는 Windows latest와 version manifest를 제공하고, 저장소 릴리즈 경로를 함께 유지합니다.

## Minimum Release Contract

1. 실제로 실행 가능한 산출물이 있어야 합니다.
2. [README.md](README.md)와 [PROJECT_STATUS.md](PROJECT_STATUS.md)가 같은 상태를 가리켜야 합니다.
3. [CHANGELOG.md](CHANGELOG.md)에 의미 있는 변경이 기록돼야 합니다.
4. 최신 스크린샷이 현재 UI를 대표해야 합니다.
5. 다운로드 경로와 릴리즈 링크가 함께 갱신돼야 합니다.
6. 공개 원격은 `브랜치 + 태그 + 릴리즈 페이지 + 자산`을 한 세트로 맞춥니다.
7. 공개 릴리즈 페이지에는 산출물과 최신 스크린샷을 함께 게시합니다.

## Platform Policy

- Windows: 빌드 산출물, 스크린샷, 체크섬을 함께 남깁니다.
- Mobile web: 라이브 반영이 있으면 스크린샷과 상태 문서를 함께 갱신합니다.
- Android: APK 공개 시 공식 미러와 저장소 릴리즈를 함께 맞춥니다.

## Public Release Sequence

1. 내부 기준선에서 산출물과 스크린샷을 먼저 고정합니다.
2. `public/*` 브랜치에 공개 가능한 이력을 정리합니다.
3. 같은 기준선에 버전 태그를 생성합니다.
4. 제2 공개 레포에 브랜치와 태그를 푸시합니다.
5. 제2 공개 레포 릴리즈 페이지에 자산과 노트를 게시합니다.
6. 명시적 요청이 있을 때만 같은 태그와 자산을 제3 공개 레포에 게시합니다.
7. `download-vstalk.phy.kr`는 최신 포인터만 유지합니다.

## Release Scripts

- 공개 기준 태그 생성: [`scripts/create-release-tag.sh`](scripts/create-release-tag.sh)
- 제2 공개 레포 릴리즈 게시: [`scripts/publish-gitea-release.sh`](scripts/publish-gitea-release.sh)
- 제3 GitHub 릴리즈 게시: [`scripts/publish-github-release.sh`](scripts/publish-github-release.sh)
- 공개 브랜치/태그/릴리즈 순차 게시: [`scripts/publish-public-release.sh`](scripts/publish-public-release.sh)

## Related Docs

- 배포 골격: [deploy/README.md](deploy/README.md)
- 릴리즈 메타데이터: [release-assets/README.md](release-assets/README.md)
- 상태표: [PROJECT_STATUS.md](PROJECT_STATUS.md)
