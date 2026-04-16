# Release Assets

이 디렉터리는 Windows와 Android 클라이언트 산출물을 함께 정리하는 멀티플랫폼 릴리즈 스테이징 영역입니다. 소스 코드 디렉터리가 아니라, 생성된 릴리즈 메타데이터와 배포 번들을 잠시 정리하는 generated surface로 취급합니다.

## 목표

- 같은 버전 번호 아래에 Windows와 Android 산출물을 병렬로 보관합니다.
- `latest/`는 최신 포인터, `releases/<version>/`는 불변 이력으로 구분합니다.
- 원격 Forge Releases는 버전별 원본 저장소, `download-vstalk.phy.kr`는 최종 사용자용 다운로드 미러로 사용합니다.

## 목표 구조

```text
release-assets/
  latest/
    version.json
    latest.json
    RELEASE_NOTES.ko.md
    SHA256SUMS.txt
    screenshots/
    windows/
      KoTalk-windows-x64.zip
      SHA256SUMS.txt
      version.json
    android/
      KoTalk-android-universal.apk
      SHA256SUMS.txt
      version.json
  releases/
    v0.2.0-alpha.1/
      version.json
      RELEASE_NOTES.ko.md
      SHA256SUMS.txt
      screenshots/
      windows/
        x64/
          KoTalk-windows-x64-v0.2.0-alpha.1.zip
          SHA256SUMS.txt
      android/
        universal/
          KoTalk-android-universal-v0.2.0-alpha.1.apk
          SHA256SUMS.txt
```

## 기본 규칙

- 같은 버전은 같은 서버 API 계약과 같은 릴리즈 노트를 공유합니다.
- Windows와 Android는 같은 태그 아래 병렬 산출물로 게시합니다.
- Windows 기본 공개 형식은 `zip`, Android 기본 공개 형식은 `apk`입니다.
- APK는 공개 채널에 올릴 때 반드시 서명본을 사용합니다.
- `latest/version.json`은 전체 플랫폼 상태를 담고, `latest/windows/version.json`, `latest/android/version.json`은 플랫폼별 상세 포인터를 담습니다.

## 다운로드 경로 규칙

- 최신 Windows: `https://download-vstalk.phy.kr/windows/latest`
- 최신 Android: `https://download-vstalk.phy.kr/android/latest`
- 전체 최신 메타데이터: `https://download-vstalk.phy.kr/latest/version.json`
- 버전별 Windows: `https://download-vstalk.phy.kr/releases/<version>/windows/x64/...`
- 버전별 Android: `https://download-vstalk.phy.kr/releases/<version>/android/universal/...`

## 생성 스크립트

실제 파일 생성은 `scripts/release/release-prepare-assets.sh`를 사용합니다.

예시:

```bash
./scripts/release/release-prepare-assets.sh \
  --version v0.2.0-alpha.1 \
  --channel alpha \
  --windows-zip artifacts/release/KoTalk-windows-x64-v0.2.0-alpha.1.zip \
  --android-apk artifacts/release/KoTalk-android-universal-v0.2.0-alpha.1.apk \
  --screenshots artifacts/screenshots \
  --force
```

## 업로드 스크립트

- VPS 다운로드 미러 업로드: `scripts/release/release-upload-assets.sh`
- Forge Releases 게시: `scripts/release/release-publish-forge.sh`
- GitHub Releases 게시: `scripts/release/release-publish-github.sh`
- 공개 원격 전체 게시: `scripts/release/release-publish-public.sh`
- 공개 태그 생성: `scripts/release/release-create-tag.sh`

두 채널은 목적이 다릅니다.

- Forge Releases: 버전별 원본 보관
- 다운로드 미러: 최신 포인터와 빠른 정적 다운로드
- 모바일 웹앱: `release-assets/`가 아니라 `vstalk.phy.kr` 배포 트랙에서 별도 운영
- 공개 원격 릴리즈 페이지에는 ZIP/APK뿐 아니라 `screenshots/` 아래 최신 캡처도 함께 게시합니다.

## 운영 메모

- 생성된 버전별 산출물은 워크트리에 유지하며, 최신 로컬 검수와 서버 업로드 기준으로 사용합니다.
- 공개 릴리즈마다 `RELEASE_NOTES.ko.md`, `SHA256SUMS.txt`, `version.json`을 함께 갱신합니다.
- 같은 버전에서 Windows만 있고 Android가 아직 없을 수는 있지만, 장기 원칙은 `같은 버전 아래 두 플랫폼 병렬 게시`입니다.
- 모바일 웹앱 정적 산출물은 `release-assets/`가 아니라 `/srv/vs-messanger/webapp/releases/<version>`에 배포합니다.
