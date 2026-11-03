using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 채집 가능 마커. GatherController가 가까운 Gatherable을 찾아 채집 시작.
    /// </summary>
    public sealed class Gatherable : MonoBehaviour
    {
        public ResourceKind YieldKind = ResourceKind.Wood;
        public int YieldAmount = 4;
        public float DurationSec = 9f;
        [Tooltip("채집 완료 시 GameObject 파괴 여부")]
        public bool DestroyOnGather = true;
        private bool _claimed;
        public bool IsTree => YieldKind == ResourceKind.Wood && GetComponent<DiscoverySite>() == null;
        public bool IsRock => YieldKind==ResourceKind.Stone && GetComponent<DiscoverySite>()==null && GetComponent<AnimalAi>()==null;
        public RockHealth EnsureRockHealth()
        {
            if(!IsRock)return null;
            return GetComponent<RockHealth>()??gameObject.AddComponent<RockHealth>();
        }
        public TreeHealth EnsureTreeHealth()
        {
            if (!IsTree) return null;
            return GetComponent<TreeHealth>() ?? gameObject.AddComponent<TreeHealth>();
        }
        private void Start()
        {
            YSort.Ensure(gameObject);EnsureTreeHealth();
            EnsureRockHealth();
            if(IsRock)RockVisualStyle.Apply(GetComponent<SpriteRenderer>());
            if(GetComponent<AnimalAi>()==null && GetComponent<DiscoverySite>()==null && (IsTree || YieldKind==ResourceKind.Stone))
                SnowContact.Attach(this);
        }

        public void OnGathered(ResourceStore store)
        {
            if (_claimed || store == null) return;
            var animal=GetComponent<AnimalAi>();
            if (animal!=null && !animal.IsDead) return;
            if (IsTree && EnsureTreeHealth().CurrentHp > 0) return;
            if (IsRock && EnsureRockHealth().CurrentHp > 0) return;
            _claimed = true;
            var discovery = GetComponent<DiscoverySite>();
            if (discovery != null)
            {
                discovery.Resolve(store);
                if (DestroyOnGather)
                {
                    GatherCompletionFx.Spawn(this, discovery.RewardKind);
                    Destroy(gameObject);
                }
                return;
            }

            Color tint = ResourceTint(YieldKind);
            int yield = YieldAmount;
            if (YieldKind == ResourceKind.Wood)
            {
                yield = Mathf.Max(1, Mathf.RoundToInt(yield * BuildingUpgradeRules.SawmillWoodYieldMultiplier()));
            }

            if (yield > 0)
            {
                if (IsTree || YieldKind==ResourceKind.Meat) WoodPickup.Spawn(transform.position,yield,store,YieldKind);
                else
                {
                    store.Add(YieldKind, yield);
                    GameFeel.FloatText(transform.position, $"+{yield} {YieldKind}", tint);
                    Sfx.Pickup();
                }
            }
            else
            {
                // 드랍 실패 (예: 늑대 75%) — 가벼운 피드백만
                GameFeel.FloatText(transform.position, "—", new Color(0.7f, 0.7f, 0.75f));
            }

            // Ore is a bonus on completed natural rock mining, separate from stone yield.
            if (YieldKind==ResourceKind.Stone && LootRules.RockDropsOre(Random.value,LootRules.OreDropChance))
                WoodPickup.Spawn(transform.position+Vector3.left*.4f,1,store,ResourceKind.Ore);

            // 보너스 자원 (예: 눈토끼는 Frostbloom 도 같이)
            var bonus = GetComponent<BonusYieldOnGather>();
            if (bonus != null && bonus.Amount > 0)
            {
                store.Add(bonus.Kind, bonus.Amount);
                GameFeel.FloatText(transform.position + new Vector3(0f, 0.3f, 0f),
                    $"+{bonus.Amount} {bonus.Kind}", ResourceTint(bonus.Kind));
            }

            if (DestroyOnGather)
            {
                if(animal==null)GatherCompletionFx.Spawn(this,YieldKind);
                else GameFeel.DeathPoof(transform.position, tint, 0.5f);
                Destroy(gameObject);
            }
        }

        private static Color ResourceTint(ResourceKind k) => k switch
        {
            ResourceKind.Wood => new Color(0.35f, 0.7f, 0.35f, 0.9f),
            ResourceKind.Stone => new Color(0.7f, 0.7f, 0.75f, 0.9f),
            ResourceKind.Meat => new Color(0.85f, 0.45f, 0.4f, 0.9f),
            ResourceKind.Food => new Color(0.95f, 0.78f, 0.4f, 0.9f),
            ResourceKind.Frostbloom => new Color(0.7f, 0.85f, 1f, 0.9f),
            _ => new Color(0.9f, 0.9f, 0.9f, 0.9f),
        };
    }
}
