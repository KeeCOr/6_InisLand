using UnityEngine;
using UnityEngine.SceneManagement;

namespace IL6
{
    public sealed class FenceWreckage : MonoBehaviour
    {
        private string _sourceSprite;
        private bool _gate,_vertical,_cleared;
        private Transform _sourceParent;
        private Quaternion _rotation;
        private Vector3 _scale=Vector3.one;
        public bool CanRestore=>!_cleared && isActiveAndEnabled;
        public static void Spawn(Building source)
        {
            var body=source.GetComponent<SpriteRenderer>();
            bool vertical=body!=null && body.sprite!=null && body.sprite.name.ToLowerInvariant().Contains("vertical");
            var sprite=Resources.Load<Sprite>("Sprites/Props/FenceWreckage20261103/"+(vertical?"vertical":"horizontal"));
            if(sprite==null)return;
            ClearAt(source.transform.position);
            var go=new GameObject("__FenceWreckage");SceneManager.MoveGameObjectToScene(go,source.gameObject.scene);
            go.transform.position=source.transform.position;
            var debris=go.AddComponent<FenceWreckage>();
            debris._sourceSprite=body!=null && body.sprite!=null?body.sprite.name:"wooden_fence_Center";
            debris._gate=source.GetComponent<Door>()!=null;debris._vertical=vertical;
            debris._sourceParent=source.transform.parent;debris._rotation=source.transform.rotation;debris._scale=source.transform.lossyScale;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingOrder=-9700;
            if(body!=null)sr.sortingLayerID=body.sortingLayerID;
        }
        public static void ClearAt(Vector3 position)
        {
            foreach(var rubble in Object.FindObjectsByType<FenceWreckage>(FindObjectsSortMode.None))
                if(Vector2.Distance(rubble.transform.position,position)<.45f)
                {rubble._cleared=true;Destroy(rubble.gameObject);}
        }
        public bool Restore()
        {
            if(!CanRestore)return false;
            _cleared=true;Vector3 p=transform.position;GameObject restored;
            if(_gate)
            {restored=VillageStarter.SpawnGate(p);restored.GetComponent<Door>().Init(_vertical,false);}
            else switch(_sourceSprite)
            {
                case "wooden_fence_Left":restored=VillageStarter.SpawnFence(p,0,FencePieceType.Left);break;
                case "wooden_fence_Right":restored=VillageStarter.SpawnFence(p,0,FencePieceType.Right);break;
                case "verticalFence_Wood_Bottom":restored=VillageStarter.SpawnVerticalFence(p,VerticalFencePieceType.Bottom);break;
                case "verticalFence_Wood_Center":restored=VillageStarter.SpawnVerticalFence(p,VerticalFencePieceType.Center);break;
                case "verticalFence_Wood_Top":restored=VillageStarter.SpawnVerticalFence(p,VerticalFencePieceType.Top);break;
                case "Fence_Wood_TopLeftCorner":restored=VillageStarter.SpawnFenceCorner(p,FenceCornerPieceType.TopLeft);break;
                case "Fence_Wood_TopRightCorner":restored=VillageStarter.SpawnFenceCorner(p,FenceCornerPieceType.TopRight);break;
                case "Fence_Wood_BottomLeftCorner":restored=VillageStarter.SpawnFenceCorner(p,FenceCornerPieceType.BottomLeft);break;
                case "Fence_Wood_BottomRightCorner":restored=VillageStarter.SpawnFenceCorner(p,FenceCornerPieceType.BottomRight);break;
                default:restored=VillageStarter.SpawnFence(p,0,FencePieceType.Center);break;
            }
            restored.transform.rotation=_rotation;restored.transform.localScale=_scale;
            if(_sourceParent!=null)restored.transform.SetParent(_sourceParent,true);
            restored.GetComponent<Building>().Initialize(BuildingKind.Fence);
            Destroy(gameObject);return true;
        }
    }
}
