using UnityEngine;
namespace IL6
{
    public sealed class SnowEmitter : MonoBehaviour
    {
        public int Rate=72, BlizzardRate=180;
        public float AreaWidth=32, TopOffset=10, FallSpeed=1.2f, BlizzardFallSpeed=3.8f;
        public float Lifetime=10, ParticleSize=.2f, BlizzardParticleSize=.25f, DriftSpeed=.35f, BlizzardDriftSpeed=3.4f;
        private readonly ParticleSystem[] _layers=new ParticleSystem[2];
        private Camera _camera; private NightController _night;
        private Material _material; private Texture2D _texture; private float _stormBlend;
        public ParticleSystem DistantSnow=>_layers[0]; public ParticleSystem NearbySnow=>_layers[1];
        private void Awake()
        {
            _camera=GetComponent<Camera>();_texture=Resources.Load<Texture2D>("Sprites/FX/Snowfall20261103/flake");
            _material=new Material(Shader.Find("Sprites/Default")){mainTexture=_texture};
            for(int i=0;i<2;i++)
            {
                var go=new GameObject(i==0?"DistantSnow":"NearbySnow");go.transform.SetParent(transform,false);
                // Put flakes inside the view volume, not on the camera's clipped z plane.
                go.transform.localPosition=Vector3.forward*(_camera!=null?Mathf.Max(2,_camera.nearClipPlane+1):5);
                var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);_layers[i]=ps;
                var main=ps.main;main.loop=true;main.prewarm=true;main.startSpeed=0;main.startLifetime=Lifetime;
                main.maxParticles=i==0?1300:320;main.simulationSpace=ParticleSystemSimulationSpace.Local;main.useUnscaledTime=false;
                var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;
                var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.Local;
                velocity.x=new ParticleSystem.MinMaxCurve(-.35f,.1f);velocity.y=new ParticleSystem.MinMaxCurve(-1.3f,-.8f);velocity.z=new ParticleSystem.MinMaxCurve(0,0);
                var noise=ps.noise;noise.enabled=true;noise.separateAxes=true;noise.strengthX=.12f;noise.strengthY=.035f;noise.strengthZ=0;noise.frequency=.35f;noise.scrollSpeed=.12f;
                var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.12f),new GradientAlphaKey(.8f,.8f),new GradientAlphaKey(0,1)});
                var color=ps.colorOverLifetime;color.enabled=true;color.color=gradient;
                var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=_material;renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.sortingOrder=10000+i;
            }
            ApplyAtmosphere();foreach(var ps in _layers){ps.Simulate(Lifetime,true,true,true);ps.Play();}
        }
        private void LateUpdate()
        {
            if(_night==null)_night=Object.FindFirstObjectByType<NightController>();
            bool storm=_night!=null && _night.CurrentPhase==Phase.Night && _night.IsBlizzard;
            _stormBlend=Mathf.MoveTowards(_stormBlend,storm?1:0,Time.deltaTime*(storm?1.8f:1.2f));ApplyAtmosphere();
        }
        private void ApplyAtmosphere()
        {
            float h=_camera!=null && _camera.orthographic?_camera.orthographicSize*2:TopOffset*2,w=_camera!=null?h*_camera.aspect:AreaWidth;
            for(int i=0;i<2;i++)
            {
                var ps=_layers[i];if(ps==null)continue;bool near=i==1;float layer=near?1.45f:.72f;
                var main=ps.main;main.startSize=new ParticleSystem.MinMaxCurve(ParticleSize*(near?.55f:.22f),Mathf.Lerp(ParticleSize,BlizzardParticleSize,_stormBlend)*(near?1:.48f));main.startColor=near?new Color(.97f,.99f,1,.85f):new Color(.86f,.92f,.99f,.70f);
                var shape=ps.shape;shape.scale=new Vector3(w*1.25f,h*1.35f,.01f);
                var emission=ps.emission;emission.rateOverTime=Mathf.Lerp(Rate,BlizzardRate,_stormBlend)*(near?.2f:.8f)*Mathf.Max(.65f,w*h/450);
                float fall=Mathf.Lerp(FallSpeed,BlizzardFallSpeed,_stormBlend)*layer,drift=Mathf.Lerp(DriftSpeed,BlizzardDriftSpeed,_stormBlend)*layer;
                var velocity=ps.velocityOverLifetime;velocity.y=new ParticleSystem.MinMaxCurve(-fall*1.15f,-fall*.75f);velocity.x=new ParticleSystem.MinMaxCurve(-drift,drift*.25f);
                var noise=ps.noise;noise.strengthX=Mathf.Lerp(.12f,.45f,_stormBlend)*layer;
            }
        }
        private void OnDisable(){foreach(var ps in _layers)if(ps!=null)ps.Pause();}
        private void OnEnable(){foreach(var ps in _layers)if(ps!=null)ps.Play();}
        private void OnDestroy(){foreach(var ps in _layers)if(ps!=null)Destroy(ps.gameObject);if(_material!=null)Destroy(_material);}
    }
}
