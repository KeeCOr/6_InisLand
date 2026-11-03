using UnityEngine;
using UnityEngine.SceneManagement;

namespace IL6
{
    /// <summary>Small scene-owned material scatter. One Start/Loop/End sequence per completed node.</summary>
    public sealed class GatherCompletionFx : MonoBehaviour
    {
        public const float Duration = .34f;
        private static int _active;
        private Sprite[] _frames;
        private SpriteRenderer _renderer;
        private float _age;

        public static void Spawn(Gatherable source, ResourceKind kind)
        {
            if(source==null || _active>=24)return;
            string material=kind==ResourceKind.Wood?"wood":kind==ResourceKind.Stone || kind==ResourceKind.Ore?"stone":"plant";
            var frames=new Sprite[3];
            string[] phases={"start","loop","end"};
            for(int i=0;i<3;i++)
            {
                frames[i]=Resources.Load<Sprite>("Sprites/FX/Gather20261103/"+material+"_"+phases[i]);
                if(frames[i]==null)return;
            }
            var go=new GameObject("__GatherCompletion");
            SceneManager.MoveGameObjectToScene(go,source.gameObject.scene);
            go.transform.position=source.transform.position+Vector3.up*.12f;
            var effect=go.AddComponent<GatherCompletionFx>();
            _active++;effect._frames=frames;
            effect._renderer=go.AddComponent<SpriteRenderer>();
            effect._renderer.sprite=frames[0];
            var body=source.GetComponent<SpriteRenderer>();
            if(body!=null)effect._renderer.sortingLayerID=body.sortingLayerID;
            effect.UpdateOrder();
        }

        private void Update()
        {
            _age+=Time.deltaTime;
            if(_age>=Duration){Destroy(gameObject);return;}
            _renderer.sprite=_frames[_age<.07f?0:_age<.2f?1:2];
            _renderer.color=new Color(1,1,1,_age<.2f?1:Mathf.Clamp01((Duration-_age)/.14f));
        }

        private void LateUpdate()=>UpdateOrder();
        private void UpdateOrder()
        {
            float cameraY=Camera.main!=null?Camera.main.transform.position.y:0;
            _renderer.sortingOrder=Mathf.Clamp(YSort.ComputeOrder(transform.position.y-cameraY)+2,-8998,9002);
        }
        private void OnDestroy()=>_active=Mathf.Max(0,_active-1);
    }
}
