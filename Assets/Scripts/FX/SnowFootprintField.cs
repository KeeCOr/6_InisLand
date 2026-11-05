using System;
using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    // World-space ground decals, retained for the whole daylight period.
    [DefaultExecutionOrder(190)]
    public sealed class SnowFootprintField : MonoBehaviour
    {
        public const int Capacity=8192;
        private const int BatchSize=256;
        public static SnowFootprintField Instance {get;private set;}
        public int Count {get;private set;}
        public bool Accepting=>_daylight && Time.timeScale>0 && !InputReader.HudBlocked;
        private readonly List<Batch> _batches=new List<Batch>();
        private Material _material;
        private bool _daylight;
        private int _day;
        private Action _unsubDay,_unsubEvening,_unsubNight;

        public static void Ensure()
        {
            if(Instance==null)new GameObject("DaytimeSnowFootprints").AddComponent<SnowFootprintField>();
        }
        private void Awake()
        {
            Instance=this;
            var cycle=GameSession.Instance!=null?GameSession.Instance.Cycle:null;
            _day=cycle!=null?cycle.Day:0;
            _daylight=cycle==null || cycle.Phase==Phase.Day;
            _material=new Material(Shader.Find("Sprites/Default"))
            {mainTexture=Resources.Load<Texture2D>("Sprites/FX/Snowfall20261103/bootprint")};
        }
        private void OnEnable()
        {
            _unsubDay=EventBus.Instance.Subscribe<DayStartedPayload>(p=>
            {if(p.Day!=_day)Clear();_day=p.Day;_daylight=true;});
            _unsubEvening=EventBus.Instance.Subscribe<EveningStartedPayload>(_=>{_daylight=false;Clear();});
            _unsubNight=EventBus.Instance.Subscribe<NightStartedPayload>(_=>{_daylight=false;Clear();});
        }
        private void OnDisable(){_unsubDay?.Invoke();_unsubEvening?.Invoke();_unsubNight?.Invoke();}
        public bool Stamp(Vector2 position,Vector2 direction,bool left)
        {
            // Preserve old daytime marks at the cap; do not recycle them under the player.
            if(!Accepting || Count>=Capacity || direction.sqrMagnitude<.001f || _material.mainTexture==null)return false;
            int batch=Count/BatchSize;
            if(batch==_batches.Count)_batches.Add(new Batch(transform,_material,batch));
            _batches[batch].Add(position,direction.normalized,left);Count++;return true;
        }
        private void LateUpdate(){foreach(var batch in _batches)batch.Upload();}
        private void Clear(){Count=0;foreach(var batch in _batches)batch.Clear();}
        private void OnDestroy()
        {
            if(Instance==this)Instance=null;
            foreach(var batch in _batches)batch.Dispose();
            if(_material!=null)Destroy(_material);
        }
        private sealed class Batch
        {
            private readonly Mesh _mesh;
            private readonly MeshRenderer _renderer;
            private readonly Vector3[] _vertices=new Vector3[BatchSize*4];
            private readonly Vector2[] _uv=new Vector2[BatchSize*4];
            private readonly Color32[] _colors=new Color32[BatchSize*4];
            private readonly int[] _triangles=new int[BatchSize*6];
            private int _used;private bool _dirty;
            public Batch(Transform parent,Material material,int index)
            {
                var go=new GameObject("FootprintBatch_"+index);go.transform.SetParent(parent,false);
                _mesh=new Mesh{name="Daytime bootprint batch"};_mesh.MarkDynamic();
                go.AddComponent<MeshFilter>().sharedMesh=_mesh;
                _renderer=go.AddComponent<MeshRenderer>();_renderer.sharedMaterial=material;
                _renderer.sortingOrder=-9400;_renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                _renderer.receiveShadows=false;
                for(int i=0;i<BatchSize;i++)
                {
                    int v=i*4,t=i*6;_triangles[t]=v;_triangles[t+1]=v+1;_triangles[t+2]=v+2;
                    _triangles[t+3]=v;_triangles[t+4]=v+2;_triangles[t+5]=v+3;
                }
            }
            public void Add(Vector2 position,Vector2 forward,bool left)
            {
                Vector2 right=new Vector2(forward.y,-forward.x)*.085f,up=forward*.16f;
                int v=_used*4;_vertices[v]=position-right-up;_vertices[v+1]=position+right-up;
                _vertices[v+2]=position+right+up;_vertices[v+3]=position-right+up;
                float a=left?1:0,b=left?0:1;
                _uv[v]=new Vector2(a,0);_uv[v+1]=new Vector2(b,0);_uv[v+2]=new Vector2(b,1);_uv[v+3]=new Vector2(a,1);
                for(int i=0;i<4;i++)_colors[v+i]=new Color32(255,255,255,235);
                _used++;_dirty=true;
            }
            public void Upload()
            {
                if(!_dirty)return;
                _mesh.SetVertices(_vertices,0,_used*4);_mesh.SetUVs(0,_uv,0,_used*4);_mesh.SetColors(_colors,0,_used*4);
                _mesh.SetTriangles(_triangles,0,_used*6,0,true);_renderer.enabled=true;_dirty=false;
            }
            public void Clear(){_used=0;_dirty=false;_mesh.Clear();_renderer.enabled=false;}
            public void Dispose(){if(_mesh!=null)UnityEngine.Object.Destroy(_mesh);}
        }
    }
}
