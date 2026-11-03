using UnityEngine;
namespace IL6
{
    // A separate ground-plane renderer avoids inheriting the prop's YSort group.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SnowContact : MonoBehaviour
    {
        private SpriteRenderer _source,_decal;
        private Gatherable _resource;
        public SpriteRenderer Decal=>_decal;
        public static void Attach(Gatherable resource)
        {
            if(resource.GetComponent<SnowContact>()==null)resource.gameObject.AddComponent<SnowContact>();
        }
        private void Awake(){_source=GetComponent<SpriteRenderer>();_resource=GetComponent<Gatherable>();}
        private void LateUpdate()
        {
            if(_source==null || _source.sprite==null || _resource==null)return;
            if(_decal==null)
            {
                var sprite=Resources.Load<Sprite>("Sprites/Environment/Snowfield/snow_contact");
                if(sprite==null)return;
                var go=new GameObject("SnowContact_"+GetInstanceID());
                _decal=go.AddComponent<SpriteRenderer>();_decal.sprite=sprite;
                _decal.sortingLayerID=_source.sortingLayerID;_decal.sortingOrder=-9800;
            }
            bool tree=_resource.IsTree;
            _decal.enabled=_source.enabled;
            _decal.color=new Color(1,1,1,Mathf.Min(.65f,_source.color.a));
            _decal.transform.position=transform.TransformPoint(new Vector3(0,tree?-.24f:-.32f,0));
            float width=tree?1.7f:1.6f*Mathf.Abs(transform.lossyScale.x);
            var size=_decal.sprite.bounds.size;
            _decal.transform.localScale=new Vector3(width/size.x,width*.35f/size.y,1);
        }
        private void OnDisable(){if(_decal!=null)_decal.enabled=false;}
        private void OnDestroy(){if(_decal!=null)Destroy(_decal.gameObject);}
    }
}
