using UnityEngine;
namespace IL6
{
    public sealed class StoryVillageRuins : MonoBehaviour
    {
        public static void Apply()
        {
            if(GameObject.Find("StoryVillageRuins")!=null) return;
            var center=new Vector3(GameConstants.VillageCenterX,GameConstants.VillageCenterY,0);
            // Only invoked by the new-game story flag; old saves never enable it implicitly.
            foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
                if(b.Kind!=BuildingKind.Campfire && b.Kind!=BuildingKind.Fence && b.Kind!=BuildingKind.House && Vector2.Distance(b.transform.position,center)<=8)
                {b.gameObject.SetActive(false);Destroy(b.gameObject);}
            // The guaranteed starting home, fire, perimeter and gate survive the story storm.
            var root=new GameObject("StoryVillageRuins");root.AddComponent<StoryVillageRuins>();
            var sprites=Resources.LoadAll<Sprite>("Sprites/Story20261026/village_ruins");
            var offsets=new[]{new Vector3(-2,2),new Vector3(3,1),new Vector3(-3,-2),new Vector3(2,-3)};
            for(int i=0;i<4;i++)
            {
                var art=new GameObject("Ruin_"+i);art.transform.SetParent(root.transform,false);art.transform.position=center+offsets[i];
                var renderer=art.AddComponent<SpriteRenderer>();
                foreach(var sprite in sprites) if(sprite.name=="ruin_"+i) renderer.sprite=sprite;
                bool occupied=false;
                foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
                {
                    if(b.CurrentHp<=0)continue;
                    if(b.Kind==BuildingKind.House)
                    {
                        var home=b.GetComponent<SpriteRenderer>();
                        if((home!=null && home.bounds.Intersects(renderer.bounds)) || Vector2.Distance(b.transform.position,art.transform.position)<1.2f){occupied=true;break;}
                    }
                    if(b.Kind==BuildingKind.Campfire && Vector2.Distance(b.transform.position,art.transform.position)<1.2f){occupied=true;break;}
                }
                if(occupied){art.SetActive(false);Destroy(art);continue;}
                YSort.Ensure(art);
            }
        }
        private float _poll;
        private void Update()
        {
            _poll+=Time.deltaTime;if(_poll<.8f) return;_poll=0;
            // A restored building replaces its nearby decorative wreckage.
            foreach(var b in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
            {
                if(b.Kind==BuildingKind.Campfire || b.Kind==BuildingKind.Fence || b.CurrentHp<=0) continue;
                foreach(Transform ruin in transform)
                    if(Vector2.Distance(b.transform.position,ruin.position)<1.2f) Destroy(ruin.gameObject);
            }
        }
    }
}
