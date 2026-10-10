# 모닝 데스크 비서

Windows 화면 위에 작게 띄워 일정 알림과 Gmail 받은편지함을 관리하는 개인용 데스크톱 앱입니다. 노란색과 검은색의 기본 스마일은 코드로 그립니다.

## 기능

- 항상 위에 표시되는 작은 위젯, 화면 안에서 이동 가능
- 일정 추가·수정·삭제, 날짜와 시각(분 단위) 지정, 알림
- Gmail 계정 여러 개 연결, 계정별/전체 받은편지함 조회
- 최근 메일을 `확인할 일`과 `읽을 소식`으로 기기 안에서 간단 분류
- Google 인증 토큰은 현재 Windows 사용자 계정에 묶어 암호화 저장

Gmail 연결은 읽기 전용 메타데이터 권한만 요청합니다. 메일을 보내거나 수정·삭제하지 않으며, 메일 내용은 외부 AI 서비스로 전송하지 않습니다. 분류는 제목과 Gmail이 제공한 짧은 미리보기의 한국어 키워드를 사용하는 초기 기능입니다. 중요 메일을 놓치지 않도록 원본 받은편지함도 확인하세요.

## 실행 및 빌드

이미 빌드된 버전은 `dist/MorningBuddy.exe`를 실행하세요. 일정을 저장하려면 `run.cmd` 또는 `dist/MorningBuddy.exe`를 실행하면 됩니다. 데이터는 `dist/morning-reminders.txt`에 저장됩니다.

Windows 기본 .NET Framework C# 컴파일러가 있는 환경에서 `build.cmd`를 실행하면 새 실행 파일을 만듭니다. 추가 NuGet 패키지는 필요하지 않습니다.

## Gmail 연결 설정

이 저장소에는 Google OAuth 클라이언트 ID가 포함되어 있지 않습니다. 앱 사용자가 직접 자신의 Google Cloud 설정을 마친 뒤 로컬 파일로 넣습니다.

1. [Google Cloud Console](https://console.cloud.google.com/)에서 프로젝트를 만들고 Gmail API를 사용 설정합니다.
2. Google Auth Platform에서 앱 이름과 지원 이메일을 설정합니다. 앱을 테스트 상태로 두는 경우 Gmail 계정을 테스트 사용자로 추가합니다.
3. OAuth 클라이언트를 만들 때 애플리케이션 유형으로 **데스크톱 앱**을 선택합니다.
4. 발급된 클라이언트 ID를 저장소 루트의 `google-client-id.example.txt`를 참고해 `google-client-id.txt` 파일에 한 줄로 넣습니다. 이 파일은 Git에 올라가지 않습니다.
5. `build.cmd`로 빌드한 다음 앱에서 **메일 → Gmail 연결**을 누르고 Google 계정을 승인합니다. 계정을 추가할 때마다 다른 계정을 선택해 여러 계정을 연결할 수 있습니다.

Google은 `gmail.metadata`를 제한된 범위로 분류합니다. 테스트 상태 앱은 동의 화면에서 테스트 사용자만 허용하며, 테스트 모드의 승인은 일정 기간 뒤 다시 필요할 수 있습니다. 개인 사용 목적이어도 Google Cloud의 현재 검증 및 테스트 정책을 확인하세요.

공식 안내: [데스크톱 앱 OAuth](https://developers.google.com/identity/protocols/oauth2/native-app), [Gmail 권한 범위](https://developers.google.com/workspace/gmail/api/auth/scopes), [Gmail 메시지 목록](https://developers.google.com/workspace/gmail/api/reference/rest/v1/users.messages/list)

## 저장 위치와 권한

- 일정: 실행 파일 폴더의 `morning-reminders.txt`
- Gmail 연결 토큰: `%LOCALAPPDATA%\MorningBuddy\gmail-accounts.dat`에 Windows 사용자용 DPAPI로 암호화 저장
- OAuth 클라이언트 ID: 사용자 로컬 파일. 저장소에는 예제만 포함
- 앱은 메일 본문을 내려받지 않고, 받은편지함의 보낸 사람·제목·짧은 미리보기를 표시합니다.

## 소스 구조

```text
morning-buddy/
├── build.cmd
├── run.cmd
├── README.md
├── .editorconfig
├── .gitignore
├── google-client-id.example.txt
├── src/
│   ├── Program.cs
│   ├── MainWindow.cs
│   ├── Reminder.cs
│   ├── ReminderDialog.cs
│   ├── ReminderStore.cs
│   ├── SmileRenderer.cs
│   ├── UiScale.cs
│   ├── NativeWindow.cs
│   ├── GmailAccount.cs
│   ├── GmailAccountStore.cs
│   ├── GmailApi.cs
│   ├── GmailManagerForm.cs
│   ├── GmailMessage.cs
│   ├── GmailOAuth.cs
│   ├── GmailOrganizer.cs
│   └── morning.manifest
└── dist/                 # 빌드 결과 및 로컬 일정 파일
```

공개 배포 시 적용할 라이선스는 저장소 소유자가 선택해 추가하세요.
