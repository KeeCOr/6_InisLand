import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.stoic.snowfield',
  appName: 'Snowfield',
  webDir: 'dist',
  server: {
    // Development only — remove before production build
    // url: 'http://192.168.x.x:5173',
    // cleartext: true,
  },
  android: {
    buildOptions: {
      releaseType: 'APK',
    },
  },
  ios: {
    contentInset: 'automatic',
  },
};

export default config;
