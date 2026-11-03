using UnityEngine;

namespace IL6
{
    /// <summary>
    /// 키보드 입력 추상화. Unity 신규 Input System 대신 기존 Input 모듈로 시작
    /// (셋업 단순화). 나중에 Input System으로 교체 가능.
    /// </summary>
    public sealed class InputReader : MonoBehaviour
    {
        public Vector2 MoveAxis { get; private set; }
        public bool InteractPressed { get; private set; }
        public static bool HudBlocked { get; set; }
        public bool CancelPressed { get; private set; }
        public bool BuildPressed { get; private set; }
        private int _interactionConsumedFrame=-1;
        public void ConsumeInteraction()
        { _interactionConsumedFrame=Time.frameCount;InteractPressed=false; }

        private void Update()
        {
            if (HudBlocked || Time.timeScale <= 0f)
            {
                MoveAxis = Vector2.zero; InteractPressed = false; BuildPressed = false; CancelPressed = false;
                return;
            }
            float ax = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
                     - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            // 2D world 좌표는 +Y가 위 → W/위 = +1, S/아래 = -1.
            // (이전에는 부호가 뒤집혀 있어서 W가 아래로 가고 S가 위로 갔던 버그)
            float ay = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f)
                     - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
            Vector2 touchMove = Vector2.zero;
            for (int i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                var touch = UnityEngine.Input.GetTouch(i);
                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                    touchMove += MobileHudLayout.MoveAt(MobileHudLayout.GuiPoint(touch.position));
            }
            if (UnityEngine.Input.touchCount == 0 && UnityEngine.Input.GetMouseButton(0))
                touchMove = MobileHudLayout.MoveAt(MobileHudLayout.GuiPoint(UnityEngine.Input.mousePosition));
            MoveAxis = Vector2.ClampMagnitude(new Vector2(ax, ay) + touchMove, 1f);

            InteractPressed = Time.frameCount!=_interactionConsumedFrame
                && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E));
            CancelPressed = Input.GetKeyDown(KeyCode.Escape);
            BuildPressed = Input.GetKeyDown(KeyCode.B);
        }
    }
}
