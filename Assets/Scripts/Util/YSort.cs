using UnityEngine;
using UnityEngine.Rendering;

namespace IL6
{
    /// <summary>
    /// Y축 월드 좌표 기반으로 SpriteRenderer 의 sortingOrder 를 설정.
    /// 아래쪽(더 큰 -y)에 있는 오브젝트가 위쪽 오브젝트보다 앞에 그려짐.
    /// </summary>
    [DefaultExecutionOrder(2000)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class YSort : MonoBehaviour
    {
        [SerializeField] private int orderOffset = 0;
        [SerializeField] private bool updateEveryFrame = true;

        private SortingGroup _group;
        private float _lastY = float.NaN, _lastCameraY = float.NaN;

        public static int ComputeOrder(float y) => Mathf.RoundToInt(-y * 100f);

        public static void Ensure(GameObject owner)
        {
            if (owner.GetComponent<SpriteRenderer>() == null) return;
            var sort = owner.GetComponent<YSort>();
            if (sort == null) sort = owner.AddComponent<YSort>();
            sort.UpdateSortingOrder();
        }
        private void Awake() { UpdateSortingOrder(); }

        private void Start()
        {
            UpdateSortingOrder();
        }

        private void LateUpdate()
        {
            float cameraY = Camera.main != null ? Camera.main.transform.position.y : 0f;
            if (updateEveryFrame || transform.position.y != _lastY || cameraY != _lastCameraY)
            {
                UpdateSortingOrder();
            }
        }

        public void SetUpdateEveryFrame(bool value)
        {
            updateEveryFrame = value;
        }

        public void UpdateSortingOrder()
        {
            if (_group == null) _group = GetComponent<SortingGroup>();
            if (_group == null) _group = gameObject.AddComponent<SortingGroup>();
            _lastY = transform.position.y;
            _lastCameraY = Camera.main != null ? Camera.main.transform.position.y : 0f;
            // Share a camera origin so distant chunks stay in front of the ground.
            // SortingGroup keeps each building and its child markers together.
            _group.sortingOrder = Mathf.Clamp(ComputeOrder(_lastY - _lastCameraY) + orderOffset, -9000, 9000);
        }

        public void SetOrderOffset(int offset)
        {
            orderOffset = offset;
            UpdateSortingOrder();
        }
    }
}
