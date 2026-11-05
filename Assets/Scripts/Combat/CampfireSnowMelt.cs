using UnityEngine;
namespace IL6
{
    public sealed class CampfireSnowMelt : MonoBehaviour
    {
        public const string SpritePath="Sprites/Environment/Campfire20261104/melted_ground";
        private CampfireAura _heat;
        private Building _building;
        private SpriteRenderer _ground;
        public SpriteRenderer GroundRenderer=>_ground;
        public float Amount {get;private set;}
        private bool Warm=>_heat!=null && _heat.isActiveAndEnabled && _heat.IsActive && _building!=null && _building.Kind==BuildingKind.Campfire && _building.CurrentHp>0;
        private void LateUpdate(){UpdateVisual(Time.deltaTime);}
        public void UpdateVisual(float elapsed)
        {
            if(_heat==null)_heat=GetComponent<CampfireAura>();
            if(_building==null)_building=GetComponent<Building>();
            if(_ground==null)
            {
                var sprite=Resources.Load<Sprite>(SpritePath);if(sprite==null)return;
                var go=new GameObject("CampfireMeltedGround");
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,gameObject.scene);
                _ground=go.AddComponent<SpriteRenderer>();_ground.sprite=sprite;_ground.sortingOrder=-9490;
                Amount=Warm?1:0;
            }
            Amount=Mathf.MoveTowards(Amount,Warm?1:0,Mathf.Max(0,elapsed)/(Warm?2f:8f));
            float width=Mathf.Clamp((_heat!=null?_heat.Radius:2.5f)*1.6f,3.6f,6.4f)*Mathf.Lerp(.85f,1,Amount);
            var size=_ground.sprite.bounds.size;
            _ground.transform.position=transform.position+Vector3.down*.05f;
            _ground.transform.localScale=new Vector3(width/size.x,width*.675f/size.y,1);
            _ground.color=new Color(1,1,1,Amount);_ground.enabled=isActiveAndEnabled && Amount>.001f;
        }
        private void OnDisable(){if(_ground!=null)_ground.enabled=false;}
        private void OnDestroy(){if(_ground!=null)Destroy(_ground.gameObject);}
    }
}
