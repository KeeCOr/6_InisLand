using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 외부 오디오 파일 없이 런타임에 사인파 + ADSR 엔벨로프로 짧은 효과음을 만들어 재생.
    /// 모든 호출은 정적 — 처음 호출 시 DontDestroyOnLoad AudioSource 를 1개 생성.
    /// 볼륨 마스터는 PlayerPrefs("il6_sfx_vol", 0.6f) 에서 읽음.
    /// </summary>
    public enum ImpactMaterial { Wood, Stone, Metal }

    public static class Sfx
    {
        private static AudioSource _src;
        private static AudioClip _cHit, _cPickup, _cDeath, _cBuild, _cClick, _cNight, _cBoss;

        private static readonly AudioClip[] _impactClips = new AudioClip[3];
        private static AudioClip _sunsetBell;
        public static AudioClip SunsetBellClip()
        {
            if(_sunsetBell!=null)return _sunsetBell;
            const int rate=22050;var data=new float[Mathf.CeilToInt(rate*.65f)];
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)rate,value=0;
                for(int note=0;note<2;note++)
                {
                    float age=t-note*.28f;if(age<0 || age>.3f)continue;
                    float frequency=note==0?660f:440f;
                    float envelope=Mathf.Clamp01(age/.008f)*Mathf.Exp(-age/.1f)*Mathf.Clamp01((.3f-age)/.04f);
                    value+=(Mathf.Sin(2*Mathf.PI*frequency*age)+.18f*Mathf.Sin(2*Mathf.PI*frequency*2.01f*age))*envelope*.4f;
                }
                data[i]=Mathf.Clamp(value,-1,1);
            }
            _sunsetBell=AudioClip.Create("SunsetWarningBell",data.Length,1,rate,false);_sunsetBell.SetData(data,0);return _sunsetBell;
        }
        public static void SunsetBell()=>Source().PlayOneShot(SunsetBellClip(),.55f*Volume);
        private static readonly float[] _lastImpact = { -100f, -100f, -100f };
        public static AudioClip ImpactClip(ImpactMaterial material)
        {
            int index = (int)material;
            if (_impactClips[index] != null) return _impactClips[index];
            const int rate = 22050;
            float duration = material == ImpactMaterial.Metal ? .16f : .11f;
            float frequency = material == ImpactMaterial.Wood ? 185f : material == ImpactMaterial.Stone ? 420f : 1450f;
            var data = new float[Mathf.CeilToInt(rate * duration)];
            uint noise = 271828u;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)rate;
                noise = unchecked(noise * 1664525u + 1013904223u);
                float n = (noise & 65535) / 32767.5f - 1f;
                float ring = Mathf.Sin(2f * Mathf.PI * frequency * t)
                    + .35f * Mathf.Sin(2f * Mathf.PI * frequency * 2.71f * t);
                float decay = material == ImpactMaterial.Metal ? .035f : .018f;
                float mix = material == ImpactMaterial.Stone ? .65f : .25f;
                float attack = Mathf.Clamp01(t / .0015f);
                float tail = Mathf.Clamp01((duration-t)/.008f);
                data[i] = Mathf.Clamp(ring * (1f-mix) + n * mix,-1f,1f)
                    * Mathf.Exp(-t/decay) * attack * tail * .65f;
            }
            var clip = AudioClip.Create("Impact_"+material,data.Length,1,rate,false);
            clip.SetData(data,0);
            return _impactClips[index] = clip;
        }
        public static void Impact(ImpactMaterial material)
        {
            int index = (int)material;
            if (Time.unscaledTime - _lastImpact[index] < .065f) return;
            _lastImpact[index] = Time.unscaledTime;
            Source().PlayOneShot(ImpactClip(material), .65f * Volume);
        }

        private static AudioSource Source()
        {
            if (_src != null) return _src;
            var go = new GameObject("__SfxRunner");
            Object.DontDestroyOnLoad(go);
            _src = go.AddComponent<AudioSource>();
            _src.playOnAwake = false;
            _src.spatialBlend = 0f; // 2D
            return _src;
        }

        public static float Volume
        {
            get => PlayerPrefs.GetFloat("il6_sfx_vol", 0.6f);
            set { PlayerPrefs.SetFloat("il6_sfx_vol", Mathf.Clamp01(value)); PlayerPrefs.Save(); }
        }

        public static void Hit()       { _cHit ??= MakeSine(820f, 0.06f, 0.04f);    Source().PlayOneShot(_cHit, 0.7f * Volume); }
        public static void Pickup()    { _cPickup ??= MakeSine(1320f, 0.08f, 0.06f); Source().PlayOneShot(_cPickup, 0.5f * Volume); }
        public static void Death()     { _cDeath ??= MakeSweep(440f, 110f, 0.18f);  Source().PlayOneShot(_cDeath, 0.6f * Volume); }
        public static void Build()     { _cBuild ??= MakeSine(560f, 0.10f, 0.08f);  Source().PlayOneShot(_cBuild, 0.55f * Volume); }
        public static void Click()     { _cClick ??= MakeSine(660f, 0.04f, 0.03f);  Source().PlayOneShot(_cClick, 0.4f * Volume); }
        public static void NightHowl() { _cNight ??= MakeSweep(220f, 95f, 0.55f);   Source().PlayOneShot(_cNight, 0.45f * Volume); }
        public static void Boss()      { _cBoss ??= MakeSweep(140f, 60f, 0.8f);     Source().PlayOneShot(_cBoss, 0.7f * Volume); }

        private static AudioClip MakeSine(float freq, float dur, float decay)
        {
            int sr = 22050;
            int n = Mathf.Max(8, Mathf.CeilToInt(sr * dur));
            var data = new float[n];
            float twoPi = Mathf.PI * 2f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Exp(-t / Mathf.Max(0.001f, decay));
                data[i] = Mathf.Sin(twoPi * freq * t) * env;
            }
            var c = AudioClip.Create("blip", n, 1, sr, false);
            c.SetData(data, 0);
            return c;
        }

        private static AudioClip MakeSweep(float fromHz, float toHz, float dur)
        {
            int sr = 22050;
            int n = Mathf.Max(8, Mathf.CeilToInt(sr * dur));
            var data = new float[n];
            float twoPi = Mathf.PI * 2f;
            float phase = 0f;
            float invSr = 1f / sr;
            for (int i = 0; i < n; i++)
            {
                float k = i / (float)n;
                float freq = Mathf.Lerp(fromHz, toHz, k);
                phase += twoPi * freq * invSr;
                float env = (1f - k) * (1f - k);
                data[i] = Mathf.Sin(phase) * env;
            }
            var c = AudioClip.Create("sweep", n, 1, sr, false);
            c.SetData(data, 0);
            return c;
        }
    }
}
