using UnityEngine;

namespace IL6
{
    /// <summary>Two continuously looping sources overlap when the time-of-day music changes.</summary>
    public sealed class MusicPlayback : MonoBehaviour
    {
        private const float FadeSeconds = .8f;
        private readonly AudioSource[] _sources = new AudioSource[2];
        private readonly float[] _gains = new float[2];
        private float _volume, _elapsed, _fromGain;
        private int _outgoing, _incoming;
        private bool _hasTrack, _fading;
        private AudioClip _target;

        private void Awake()
        {
            for (int i=0;i<2;i++)
            {
                var source=gameObject.AddComponent<AudioSource>();
                source.playOnAwake=false;source.loop=true;source.spatialBlend=0f;
                source.volume=0f;
                _sources[i]=source;
            }
        }

        public void SetVolume(float volume)
        {
            _volume=Mathf.Clamp01(volume);
            ApplyVolumes();
        }

        public void Play(AudioClip clip)
        {
            if (clip==null || (_hasTrack && _target==clip)) return;
            _target=clip;
            if (!_hasTrack)
            {
                _hasTrack=true;_fading=false;
                _sources[0].clip=clip;_sources[0].Play();
                _gains[0]=1f;_gains[1]=0f;ApplyVolumes();
                return;
            }
            _outgoing=_gains[1]>_gains[0]?1:0;
            _incoming=1-_outgoing;
            _fromGain=_gains[_outgoing];
            _sources[_incoming].Stop();
            _sources[_incoming].clip=clip;
            _gains[_incoming]=0f;
            ApplyVolumes();
            _sources[_incoming].Play();
            _elapsed=0f;_fading=true;
        }

        private void Update()
        {
            if (!_fading) return;
            _elapsed+=Time.unscaledDeltaTime;
            float t=Mathf.SmoothStep(0f,1f,Mathf.Clamp01(_elapsed/FadeSeconds));
            // Equal-power gains avoid the volume dip of a linear crossfade.
            _gains[_outgoing]=_fromGain*Mathf.Cos(t*Mathf.PI*.5f);
            _gains[_incoming]=Mathf.Sin(t*Mathf.PI*.5f);
            if (_elapsed>=FadeSeconds)
            {
                _gains[_outgoing]=0f;_gains[_incoming]=1f;
                _sources[_outgoing].Stop();_fading=false;
            }
            ApplyVolumes();
        }

        public void Stop()
        {
            _hasTrack=false;_fading=false;_target=null;
            for (int i=0;i<2;i++) { _sources[i].Stop();_gains[i]=0f; }
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            for (int i=0;i<2;i++) if (_sources[i]!=null) _sources[i].volume=_gains[i]*_volume;
        }
    }
}
