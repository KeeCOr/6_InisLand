# Snowfield — 모바일 출시 가이드

## Android (Google Play)

### 준비 완료
- [x] `@capacitor/android` 플랫폼 추가 완료 (`android/` 디렉토리)
- [x] `capacitor.config.ts` 설정 완료 (`com.stoic.snowfield`)
- [x] 빌드 스크립트: `npm run cap:android`

### 남은 작업

1. **Android Studio 설치** — [developer.android.com/studio](https://developer.android.com/studio)

2. **앱 서명 Keystore 생성**
   ```bash
   keytool -genkey -v -keystore snowfield-release.jks \
     -alias snowfield -keyalg RSA -keysize 2048 -validity 10000
   ```
   생성된 `.jks` 파일은 절대 커밋하지 말 것 (`.gitignore`에 추가)

3. **Keystore 경로 설정** — `android/app/build.gradle`에 signing config 추가:
   ```groovy
   android {
     signingConfigs {
       release {
         storeFile file("../../snowfield-release.jks")
         storePassword "YOUR_STORE_PASSWORD"
         keyAlias "snowfield"
         keyPassword "YOUR_KEY_PASSWORD"
       }
     }
     buildTypes {
       release {
         signingConfig signingConfigs.release
       }
     }
   }
   ```

4. **아이콘 / 스플래시 교체**
   - 앱 아이콘: `android/app/src/main/res/mipmap-*/ic_launcher.png`
   - 스플래시: `android/app/src/main/res/drawable/splash.png`
   - Capacitor Assets 도구: `npx @capacitor/assets generate`

5. **Release APK/AAB 빌드**
   ```bash
   cd android
   ./gradlew bundleRelease   # Google Play용 AAB
   ./gradlew assembleRelease  # 직접 배포용 APK
   ```

6. **Google Play Console 제출**
   - 앱 번들 업로드: `android/app/build/outputs/bundle/release/app-release.aab`
   - 콘텐츠 등급: IARC 설문 완료 필요
   - 개인정보처리방침 URL 필요

---

## iOS (App Store)

iOS 빌드는 **macOS + Xcode**가 필요합니다. Windows에서는 설정 파일만 준비 가능합니다.

### macOS에서 실행할 명령어
```bash
npm run build
npx cap add ios
npx cap open ios    # Xcode 열기
```

### iOS 출시 체크리스트
- [ ] Apple Developer 계정 ($99/year)
- [ ] Xcode에서 Bundle ID `com.stoic.snowfield` 설정
- [ ] 앱 아이콘 (1024×1024 PNG)
- [ ] 스플래시 스크린
- [ ] App Store Connect에서 앱 등록
- [ ] TestFlight 내부 테스트
- [ ] 심사 제출

---

## 앱 정보

| 항목 | 값 |
|------|-----|
| App ID (Android) | `com.stoic.snowfield` |
| App ID (iOS) | `com.stoic.snowfield` |
| 앱 이름 | Snowfield |
| 버전 | 0.1.0 |
| 콘텐츠 등급 | 전체이용가 (목표) |
| 인앱 구매 | 없음 |
| 온라인 기능 | 없음 |
