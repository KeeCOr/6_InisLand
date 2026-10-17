using UnityEngine;
namespace IL6
{
    [RequireComponent(typeof(Gatherable))]
    public sealed class TreeHealth : MonoBehaviour
    {
        public int MaxHp = 100;
        public int CurrentHp { get; private set; }
        private void Awake()
        {
            CurrentHp = MaxHp;
            var bar = GetComponent<HpBarUi>() ?? gameObject.AddComponent<HpBarUi>();
            bar.Tree = this; bar.Offset = new Vector2(0, 1.35f); bar.FillColor = new Color(.9f,.72f,.3f);
        }
        public void Chop(int damage, ResourceStore store)
        {
            if (CurrentHp <= 0 || damage <= 0 || store == null) return;
            CurrentHp = Mathf.Max(0, CurrentHp - damage);
            Sfx.Hit();
            GameFeel.FloatText(transform.position + Vector3.up, $"-{damage}", new Color(1f,.85f,.4f));
            if (CurrentHp == 0) GetComponent<Gatherable>().OnGathered(store);
        }
    }
}
