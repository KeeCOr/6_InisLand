using UnityEngine;

namespace IL6
{
    // The scene uses a solid snow sprite, rather than a boundless tilemap.
    // Follow the final camera position so travel and wider viewports never expose its edge.
    [DefaultExecutionOrder(1000)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class GroundCoverage : MonoBehaviour
    {
        public Camera View;
        private SpriteRenderer _ground;
        private void Awake() { _ground=GetComponent<SpriteRenderer>(); }
        private void LateUpdate()
        {
            if(View==null) View=Camera.main;
            if(View==null || !View.orthographic || _ground.sprite==null) return;
            float height=Mathf.Max(50f,View.orthographicSize*2f+8f);
            float width=Mathf.Max(50f,View.orthographicSize*2f*View.aspect+8f);
            var spriteSize=_ground.sprite.bounds.size;
            if(spriteSize.x<=0f || spriteSize.y<=0f) return;
            transform.position=new Vector3(View.transform.position.x,View.transform.position.y,transform.position.z);
            transform.localScale=new Vector3(width/spriteSize.x,height/spriteSize.y,1f);
        }
    }
}
