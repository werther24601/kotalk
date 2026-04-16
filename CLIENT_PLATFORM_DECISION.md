# Client Platform Decision

마지막 정리일: `2026-04-16`

## 결론

KoTalk의 장기적인 네이티브 클라이언트 기준선은 **Avalonia 중심**으로 유지하는 편이 맞습니다.  
현재 Android의 WebView 셸은 `alpha` 단계의 빠른 APK 배포를 위한 전술적 경로로 유지하되, iOS와 Linux까지 포함하는 장기 클라이언트 전략은 Avalonia를 축으로 정리합니다.

## 왜 다시 판단했는가

초기에는 `Windows + mobile web + Android APK`만 빨리 닫는 것이 우선이었습니다. 그런데 이제 조건이 바뀌었습니다.

- Android만이 아니라 iOS도 예정돼 있습니다.
- Linux 공식 지원도 예정돼 있습니다.
- Windows는 이미 네이티브 데스크톱 축으로 유지해야 합니다.
- 저장소와 공식 서비스 모두에서 UI/UX 일관성을 더 강하게 요구받고 있습니다.

즉, Android만 빨리 붙이는 기준이 아니라 `Windows + Android + iOS + Linux`를 함께 수용하는 클라이언트 구조가 필요해졌습니다.

## 현재 기준선

### 지금 이미 있는 것

- Windows 네이티브 데스크톱 앱: Avalonia 기반
- Mobile web: 실제 운영 중
- Android: WebView 기반 첫 APK 셸 생성 가능

### 지금 없는 것

- iOS 클라이언트
- Linux 네이티브 패키징
- Android/iOS 전용 네이티브 경험을 공용 UI 축으로 가져가는 체계

## 검토한 선택지

### 1. Android/iOS를 계속 각각 별도 네이티브 셸로 유지

장점:

- 지금 당장 APK를 빠르게 배포하기 쉽다
- 모바일 웹을 감싸는 방식이라 초기 유지 비용이 낮다

한계:

- Windows와 Linux 쪽 UI 자산을 재사용하기 어렵다
- iOS까지 같은 방식으로 늘어나면 표면은 빠르게 늘지만 제품 일관성은 약해진다
- 장기적으로는 플랫폼마다 다른 껍데기를 유지하게 된다

판단:

- **전술적으로는 유효**
- **장기 기준선으로는 부적합**

### 2. .NET MAUI로 장기 축 전환

장점:

- Android와 iOS, Windows까지 한 프레임워크로 묶기 좋다
- 모바일 장치 API 접근이 익숙한 생태계다

한계:

- Microsoft 공식 문서 기준 `.NET MAUI`의 기본 지원 플랫폼은 Android, iOS, macOS, Windows다. Linux는 기본 축에 없다.
- 현재 저장소의 Windows 클라이언트는 이미 Avalonia 기반이라, MAUI로 옮기면 데스크톱 축을 다시 크게 재구성해야 한다.

판단:

- **Linux 공식 지원 예정 조건과 현재 코드베이스 기준을 함께 놓고 보면, 주 프레임워크로 선택하기 어렵다**

참고:

- Microsoft Learn, “What is .NET MAUI?”: <https://learn.microsoft.com/en-us/dotnet/maui/what-is-maui>
- Microsoft Learn, “Supported platforms for .NET MAUI apps”: <https://learn.microsoft.com/en-us/dotnet/maui/supported-platforms?view=net-maui-9.0>

### 3. Uno Platform으로 장기 축 전환

장점:

- 공식 문서 기준 Android, iOS, Web, macOS, Linux, Windows를 포괄한다
- 광범위한 플랫폼 표면 자체는 매력적이다

한계:

- 현재 Windows 클라이언트가 Avalonia 기반이라, Uno로 가면 데스크톱 UI 자산과 운영 경험을 다시 맞춰야 한다
- 지금 시점의 저장소 기준으로는 전환 비용이 작지 않다

판단:

- **대안으로는 충분히 검토 가능**
- **하지만 현재 코드베이스를 고려하면 1순위는 아님**

참고:

- Uno Platform, “Supported platforms”: <https://platform.uno/docs/articles/getting-started/requirements.html>

### 4. Avalonia를 장기 축으로 유지

장점:

- 현재 Windows 클라이언트가 이미 Avalonia 기반이다
- 공식 문서 기준 Windows, Linux, iOS, Android, WebAssembly까지 포괄한다
- 데스크톱 밀도, 멀티 윈도우, 플랫한 커스텀 UI를 유지하기 유리하다
- Linux 공식 지원 예정이라는 조건과 가장 자연스럽게 맞는다

전제 조건:

- Avalonia 공식 문서 기준 데스크톱은 `.NET 8` 이상이지만, 모바일인 iOS/Android는 `.NET 10` 기준을 따른다
- 따라서 현재 저장소의 `.NET 8` 기준선을 유지한 채 즉시 Android/iOS를 Avalonia로 통합하는 것은 현실적이지 않다

주의:

- 모바일은 데스크톱과 같은 방식으로 밀어붙이면 안 된다
- 작은 화면, 제스처, OS 관례를 반영한 모바일 전용 UX 조정은 별도로 필요하다
- 현재 Android alpha 셸을 곧바로 폐기할 필요는 없지만, 장기적으로는 공용 UI 구조와의 관계를 분명히 해야 한다

판단:

- **현 시점 KoTalk의 가장 현실적인 장기 기준선**

참고:

- Avalonia Docs, “Supported platforms”: <https://docs.avaloniaui.net/docs/supported-platforms>
- Avalonia Docs, “Welcome / What is Avalonia?”: <https://docs.avaloniaui.net/docs/welcome>

## 최종 결정

### 전술

- Android는 당분간 현재 WebView 셸을 유지합니다.
- 이유는 APK 배포, 설치 동선 검증, 알파 피드백 수집을 빠르게 계속하기 위해서입니다.
- 현재 저장소가 `.NET 8` 기준선이므로, 모바일 공용 UI로 바로 넘어가기 전까지는 이 경로가 가장 안전합니다.

### 전략

- 장기적인 네이티브 클라이언트 축은 **Avalonia 기반 공유 구조**로 정리합니다.
- 다만 이 단계는 `.NET 10` 모바일 기준선을 감당할 준비가 됐을 때 들어갑니다.
- Windows와 Linux는 같은 데스크톱 계열 기준으로 묶습니다.
- Android와 iOS는 같은 제품 경험 원칙 아래로 수렴시키되, 모바일 전용 UX 조정은 별도 계층으로 둡니다.

## 배포 원칙

- 자체 미러와 공개 저장소 Assets:
  - Windows installer / onefile / zip
  - Android APK
- Apple 배포 채널:
  - iOS는 저장소 Assets나 자체 미러 직접 배포가 아니라 Apple 채널을 전제로 준비합니다

## 지금 바로 바뀌는 문장

- `Android는 계획 단계`라고 쓰지 않습니다.
- `Android APK 기준선은 확보됐고, 장기 모바일 전략은 재정의 중`이라고 씁니다.
- `iOS와 Linux는 예정`이라고 공개적으로 적되, 아직 완성된 것처럼 쓰지 않습니다.

## 다음 실행 항목

1. Android alpha 셸을 유지하면서 배포와 설치 루프를 먼저 안정화
2. `.NET 10` 전환 부담과 시점을 포함해 Avalonia 기반 공유 클라이언트 구조를 설계
3. Linux 패키징 기준선 정리
4. iOS 배포 준비 문서와 빌드 전제 정리
