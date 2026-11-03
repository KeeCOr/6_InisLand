using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace IL6
{
    [DefaultExecutionOrder(2500)]
    public sealed class HitImpact : MonoBehaviour
    {
        private static Sprite[] _frames;
        private static int _active;
        private SpriteRenderer _renderer,_source;
        private SortingGroup _sourceGroup;
        private float _age,_size;
        public static void Spawn(SpriteRenderer source)
        {
            if(source==null || !source.enabled || _active>=48)return;
            var emitter=source.GetComponent<HitImpactEmitter>()??source.gameObject.AddComponent<HitImpactEmitter>();
            if(Time.time<emitter.NextImpact)return;
            if(_frames==null || _frames.Length!=3)
            {_frames=Resources.LoadAll<Sprite>("Sprites/FX/Hit20261103/impact");Array.Sort(_frames,(a,b)=>string.CompareOrdinal(a.name,b.name));}
            if(_frames.Length!=3)return;
            emitter.NextImpact=Time.time+.06f;
            var go=new GameObject("__HitImpact");SceneManager.MoveGameObjectToScene(go,source.gameObject.scene);
            go.transform.position=source.bounds.center;
            var effect=go.AddComponent<HitImpact>();_active++;
            effect._source=source;effect._sourceGroup=source.GetComponentInParent<SortingGroup>();
            effect._size=Mathf.Clamp(source.bounds.size.magnitude*.7f,.65f,1.4f);
            effect._renderer=go.AddComponent<SpriteRenderer>();effect._renderer.sprite=_frames[0];
            effect._renderer.sortingLayerID=source.sortingLayerID;
            effect._renderer.sortingOrder=effect._sourceGroup!=null?effect._sourceGroup.sortingOrder+1:source.sortingOrder+1;
            go.transform.localScale=Vector3.one*effect._size;
        }
        private void Update()
        {
            _age+=Time.deltaTime;if(_age>=.24f){Destroy(gameObject);return;}
            _renderer.sprite=_frames[_age<.04f?0:_age<.13f?1:2];
            _renderer.color=new Color(1,1,1,_age<.13f?1:Mathf.Clamp01((.24f-_age)/.11f));
            transform.localScale=Vector3.one*_size*Mathf.Lerp(.85f,1.15f,_age/.24f);
        }
        private void LateUpdate()
        {
            if(_source!=null)transform.position=_source.bounds.center;
            if(_sourceGroup!=null)_renderer.sortingOrder=_sourceGroup.sortingOrder+1;
        }
        private void OnDestroy(){_active=Mathf.Max(0,_active-1);}
    }
    public sealed class HitImpactEmitter : MonoBehaviour { public float NextImpact; }
}
