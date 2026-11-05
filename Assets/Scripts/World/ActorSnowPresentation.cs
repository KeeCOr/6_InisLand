using UnityEngine;
namespace IL6
{
    [DefaultExecutionOrder(120)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ActorSnowPresentation : MonoBehaviour
    {
        private static Material _material;
        private SpriteRenderer _body,_shadow;
        private string _species;
        private bool _flying;
        public SpriteRenderer Shadow=>_shadow;
        public static ActorSnowPresentation Attach(SpriteRenderer body,string species=null)
        {
            var visual=body.GetComponent<ActorSnowPresentation>()??body.gameObject.AddComponent<ActorSnowPresentation>();
            visual._body=body;visual._species=species;visual._flying=species=="hawk";
            if(_material==null)_material=Resources.Load<Material>("Materials/SnowActor");
            if(_material!=null)body.sharedMaterial=_material;
            return visual;
        }
        private void LateUpdate()
        {
            if(_body==null)return;
            if(_shadow==null)
            {
                var sprite=Resources.Load<Sprite>("Sprites/Environment/Snowfield/actor_shadow");if(sprite==null)return;
                var go=new GameObject("ActorShadow_"+GetInstanceID());_shadow=go.AddComponent<SpriteRenderer>();
                _shadow.sprite=sprite;_shadow.sortingLayerID=_body.sortingLayerID;_shadow.sortingOrder=-9475;
            }
            _shadow.enabled=_body.enabled;
            float width=string.IsNullOrEmpty(_species)?.7f:Mathf.Clamp(AnimalSizeProfile.For(_species).FootprintRadius*3f,.25f,2.8f);
            if(_flying)width=.55f;
            _shadow.color=new Color(.32f,.42f,.52f,(_flying?.08f:.16f)*_body.color.a);
            _shadow.transform.position=transform.position+Vector3.down*(_flying?.65f:.025f);
            _shadow.transform.localScale=new Vector3(width,width,1f);
        }
        private void OnDisable(){if(_shadow!=null)_shadow.enabled=false;}
        private void OnDestroy(){if(_shadow!=null)Destroy(_shadow.gameObject);}
    }
}
