using UnityEngine;

namespace IL6
{
    // Whole repeat tiles keep painted snow fixed in world space while covering the camera.
    [DefaultExecutionOrder(1000)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class GroundCoverage : MonoBehaviour
    {
        public Camera View;
        private SpriteRenderer _ground;
        private void Awake()
        {
            _ground=GetComponent<SpriteRenderer>();
            var snow=Resources.Load<Sprite>("Sprites/Environment/Snowfield/snow_ground");
            if(snow!=null)
            {
                _ground.sprite=snow;_ground.color=Color.white;
                _ground.drawMode=SpriteDrawMode.Tiled;_ground.tileMode=SpriteTileMode.Continuous;
                transform.localScale=Vector3.one;
            }
        }
        private void LateUpdate()
        {
            if(View==null) View=Camera.main;
            if(View==null || !View.orthographic || _ground.sprite==null) return;
            float height=Mathf.Max(50f,View.orthographicSize*2f+8f);
            float width=Mathf.Max(50f,View.orthographicSize*2f*View.aspect+8f);
            var spriteSize=_ground.sprite.bounds.size;
            if(spriteSize.x<=0f || spriteSize.y<=0f) return;
            if(_ground.drawMode==SpriteDrawMode.Tiled)
            {
                float left=Mathf.Floor((View.transform.position.x-width*.5f)/spriteSize.x)*spriteSize.x;
                float bottom=Mathf.Floor((View.transform.position.y-height*.5f)/spriteSize.y)*spriteSize.y;
                float right=Mathf.Ceil((View.transform.position.x+width*.5f)/spriteSize.x)*spriteSize.x;
                float top=Mathf.Ceil((View.transform.position.y+height*.5f)/spriteSize.y)*spriteSize.y;
                transform.position=new Vector3((left+right)*.5f,(bottom+top)*.5f,transform.position.z);
                transform.localScale=Vector3.one;_ground.size=new Vector2(right-left,top-bottom);
            }
            else
            {
                transform.position=new Vector3(View.transform.position.x,View.transform.position.y,transform.position.z);
                transform.localScale=new Vector3(width/spriteSize.x,height/spriteSize.y,1f);
            }
        }
    }
}
