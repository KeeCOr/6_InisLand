namespace IL6
{
    public readonly struct HudGuidance
    {
        public readonly string Status;
        public readonly string Risk;
        public readonly string NextAction;

        public HudGuidance(string status, string risk, string nextAction)
        {
            Status = status;
            Risk = risk;
            NextAction = nextAction;
        }
    }

    public static class HudGuidanceText
    {
        public static HudGuidance Build(Phase phase, int activeZombies, int wavePending, bool isBlizzard, int foodShortage)
        {
            if (phase == Phase.Night && isBlizzard)
            {
                return new HudGuidance(
                    "상태: 눈보라 밤",
                    $"위험: 시야 저하 · 좀비 {activeZombies} · 대기 {wavePending}",
                    "다음: 모닥불/마을 안쪽으로 모여 버티기");
            }

            if (phase == Phase.Night)
            {
                return new HudGuidance(
                    "상태: 밤 방어 중",
                    $"위험: 좀비 {activeZombies} · 대기 {wavePending}",
                    "다음: 동료를 사수로 두고 마을 입구 방어");
            }

            if (foodShortage > 0)
            {
                return new HudGuidance(
                    "상태: 보급 부족",
                    $"위험: 식량 -{foodShortage} · 밤 전 허기 누적",
                    "다음: 가까운 열매/사냥감 채집 후 귀환");
            }

            return new HudGuidance(
                "상태: 탐험 가능",
                "위험: 즉시 위협 낮음",
                "다음: 나무/돌 확보 또는 새 거점 정찰");
        }
    }
}