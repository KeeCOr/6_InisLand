namespace IL6
{
    public sealed class HudGuidanceSlot
    {
        public string Id { get; }
        public string Label { get; }
        public string Text { get; }
        public int Priority { get; }
        public string Tone { get; }

        public HudGuidanceSlot(string id, string label, string text, int priority, string tone)
        {
            Id = id;
            Label = label;
            Text = text;
            Priority = priority;
            Tone = tone;
        }

        public string DisplayText => $"{Label}: {Text}";
    }

    public sealed class HudGuidance
    {
        public HudGuidanceSlot Objective { get; }
        public HudGuidanceSlot ImmediateRisk { get; }
        public HudGuidanceSlot RecommendedAction { get; }
        public HudGuidanceSlot[] TopPrioritySlots { get; }

        public string Status => Objective.DisplayText;
        public string Risk => ImmediateRisk.DisplayText;
        public string NextAction => RecommendedAction.DisplayText;

        public HudGuidance(HudGuidanceSlot objective, HudGuidanceSlot immediateRisk, HudGuidanceSlot recommendedAction)
        {
            Objective = objective;
            ImmediateRisk = immediateRisk;
            RecommendedAction = recommendedAction;
            TopPrioritySlots = new[] { objective, immediateRisk, recommendedAction };
        }
    }

    public static class HudGuidanceText
    {
        public static HudGuidance Build(Phase phase, int activeZombies, int wavePending, bool isBlizzard, int foodShortage)
        {
            if (phase == Phase.Night && isBlizzard)
            {
                return new HudGuidance(
                    Objective("Survive the blizzard night"),
                    Risk($"visibility low · {activeZombies} active · {wavePending} pending", "danger"),
                    Action("Return to the campfire and group villagers inside the safe zone"));
            }

            if (phase == Phase.Night)
            {
                return new HudGuidance(
                    Objective("Hold the village line"),
                    Risk($"{activeZombies} active zombies · {wavePending} pending", "danger"),
                    Action("Keep companions near the gate and defend the village entrance"));
            }

            if (foodShortage > 0)
            {
                return new HudGuidance(
                    Objective("Restore food supplies"),
                    Risk($"food -{foodShortage} · hunger will slow the next night", "warning"),
                    Action("Gather berries or hunt before scouting deeper"));
            }

            return new HudGuidance(
                Objective("Explore and stockpile"),
                Risk("No immediate threat", "safe"),
                Action("Gather wood, stone, or scout the next shelter"));
        }

        private static HudGuidanceSlot Objective(string text)
        {
            return new HudGuidanceSlot("objective", "OBJECTIVE", text, 0, "primary");
        }

        private static HudGuidanceSlot Risk(string text, string tone)
        {
            return new HudGuidanceSlot("risk", "RISK", text, 1, tone);
        }

        private static HudGuidanceSlot Action(string text)
        {
            return new HudGuidanceSlot("action", "ACTION", text, 2, "action");
        }
    }
}
