using UnityEngine;
namespace IL6
{
    public static class VillageSquareLayout
    {
        // Keep the starter square open for rebuilding. Preserve harvestable resources outside it.
        public static void MoveNaturalResourcesToOutskirts(Vector2 center, float halfSize)
        {
            var resources=Object.FindObjectsByType<Gatherable>(FindObjectsSortMode.InstanceID);
            foreach(var resource in resources)
            {
                if(resource.GetComponent<DiscoverySite>()!=null || resource.GetComponent<AnimalAi>()!=null) continue;
                if(!resource.IsTree && resource.YieldKind!=ResourceKind.Stone) continue;
                Vector2 delta=(Vector2)resource.transform.position-center;
                if(Mathf.Abs(delta.x)>halfSize+1f || Mathf.Abs(delta.y)>halfSize+1f) continue;
                float start=Mathf.Atan2(delta.y,delta.x);
                for(int attempt=0;attempt<64;attempt++)
                {
                    float angle=start+attempt*2.399963f;
                    Vector2 direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                    float extent=halfSize+3f+(attempt/16)*1.5f;
                    Vector2 offset=direction*(extent/Mathf.Max(Mathf.Abs(direction.x),Mathf.Abs(direction.y)));
                    if(offset.y<0 && Mathf.Abs(offset.x)<2f) continue; // South gate approach.
                    Vector2 point=center+offset;
                    bool blocked=false;
                    foreach(var hit in Physics2D.OverlapCircleAll(point,1.1f))
                        if(hit.gameObject!=resource.gameObject && !hit.isTrigger) {blocked=true;break;}
                    if(blocked) continue;
                    resource.transform.position=new Vector3(point.x,point.y,resource.transform.position.z);
                    Physics2D.SyncTransforms();
                    break;
                }
            }
        }
    }
}
