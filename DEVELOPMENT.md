# Development Guide

## Naming Note

공개 브랜드는 `KoTalk`이지만, 현재 저장소의 프로젝트 파일과 네임스페이스는 아직 `PhysOn.*`를 사용합니다.
문서 개편이 먼저 진행 중이며, 코드 네임스페이스 정렬은 별도 작업으로 다룹니다.

## Requirements

- `.NET 8 SDK`
- Git
- Node.js 20+
- Windows portable 빌드를 만들려면 `win-x64` publish 가능한 .NET 환경

Android 채널을 다룰 때는 아래가 추가로 필요합니다.

- `OpenJDK 17+`
- `.NET Android workload`
- Android SDK / cmdline-tools

## Quick Start

```bash
git clone <repository-url>
cd vs-messanger
dotnet build PhysOn.sln -c Debug
```

## Run The API

```bash
dotnet run --project src/PhysOn.Api --urls http://127.0.0.1:5082
```

기본 확인 URL:

- [http://127.0.0.1:5082/health](http://127.0.0.1:5082/health)
- [http://127.0.0.1:5082/](http://127.0.0.1:5082/)

접근 게이트나 시드 값은 공개 문서에 고정하지 않습니다. 필요한 값은 로컬 환경 변수나 비공개 배포 설정에서 넣어야 합니다.

## Run The Desktop Client

```bash
dotnet run --project src/PhysOn.Desktop
```

기본 입력값:

- 서버 주소: `http://127.0.0.1:5082`

## Run The Mobile Web Client

```bash
cd src/PhysOn.Web
npm install
npm run dev
```

기본 개발 주소:

- 웹앱: [http://127.0.0.1:4173](http://127.0.0.1:4173)
- API 프록시 기본값: [http://127.0.0.1:5082](http://127.0.0.1:5082)

## Test

```bash
dotnet test tests/PhysOn.Api.IntegrationTests/PhysOn.Api.IntegrationTests.csproj
```

필요 시 전체 확인:

```bash
dotnet build PhysOn.sln -c Debug
dotnet test PhysOn.sln -c Debug
```

## Release Builds

Windows:

```bash
dotnet publish src/PhysOn.Desktop/PhysOn.Desktop.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -o artifacts/release/v0.1.0-alpha.1-win-x64
```

Android:

```bash
dotnet workload install android
dotnet publish src/PhysOn.Mobile.Android/PhysOn.Mobile.Android.csproj \
  -c Release \
  -f net8.0-android \
  -p:AndroidPackageFormat=apk \
  -o artifacts/release/android
```

공개 산출물 네이밍은 `KoTalk-*` 기준으로 정리하는 방향이고, 현재 내부 스크립트와 프로젝트명은 별도 정렬 단계에 있습니다.

## Release Metadata

```bash
./scripts/release/release-prepare-assets.sh \
  --version v0.1.0-alpha.1 \
  --channel alpha \
  --windows-zip artifacts/release/PhysOn-win-x64-v0.1.0-alpha.1.zip \
  --android-apk artifacts/release/PhysOn-android-universal-v0.1.0-alpha.1.apk \
  --screenshots artifacts/screenshots \
  --force
```

## Deployment Notes

- 공개 웹 진입점: [vstalk.phy.kr](https://vstalk.phy.kr)
- 공식 다운로드 미러: [download-vstalk.phy.kr](https://download-vstalk.phy.kr)
- 저장소 릴리즈 경로: [RELEASING.md](RELEASING.md)

실제 호스트 주소, 관리자 계정, 배포용 비밀값은 공개 문서에 적지 않습니다.

## Troubleshooting

### Desktop window does not open on Linux/WSL

- X 서버 또는 데스크톱 세션이 있는지 확인합니다.
- GUI가 없는 환경이면 API와 테스트만 먼저 확인합니다.

### Download links do not open

- [download-vstalk.phy.kr](https://download-vstalk.phy.kr) DNS와 HTTPS 상태를 확인합니다.
- 저장소 릴리즈 경로가 최신인지 함께 확인합니다.

### Web app does not open

- [vstalk.phy.kr](https://vstalk.phy.kr) DNS와 프록시 상태를 확인합니다.
- 정적 파일 배포 루트가 맞는지 확인합니다.
