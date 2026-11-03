using UnityEngine;
namespace IL6
{
    [RequireComponent(typeof(Gatherable))]
    public sealed class RockHealth : MonoBehaviour
    {
        public int MaxHp {get;private set;}
        public int CurrentHp {get;private set;}
        private float _work;
        private Gatherable _node;
        private void Awake()
        {
            _node=GetComponent<Gatherable>();
            MaxHp=Mathf.Max(1,BalanceConfig.Instance.RockMaxHp);CurrentHp=MaxHp;
            var bar=GetComponent<HpBarUi>()??gameObject.AddComponent<HpBarUi>();
            bar.Tree=null;bar.Rock=this;bar.FillColor=new Color(.62f,.76f,.85f);
        }
        // Four 25-HP impacts used to finish the six-second rock. 200 HP now takes eight.
        public void Mine(float workSeconds,ResourceStore store)
        {
            if(CurrentHp<=0 || workSeconds<=0 || store==null)return;
            _work+=workSeconds;
            float interval=Mathf.Max(.2f,_node.DurationSec/4f);
            while(_work>=interval && CurrentHp>0)
            {
                _work-=interval;Hit(25,store);
            }
        }
        public void Hit(int damage,ResourceStore store)
        {
            if(CurrentHp<=0 || damage<=0 || store==null)return;
            CurrentHp=Mathf.Max(0,CurrentHp-damage);
            Sfx.Impact(ImpactMaterial.Stone);
            if(CurrentHp>0)GameFeel.MaterialImpact(transform.position,ImpactMaterial.Stone);
            GameFeel.FloatText(transform.position+Vector3.up*.5f,"-"+damage,new Color(.72f,.83f,.9f));
            if(CurrentHp==0)_node.OnGathered(store);
        }
    }
}
