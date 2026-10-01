namespace IL6
{
    public struct DailyObjective { public string Goal; public string Risk; public string FirstAction; }
    public static class DailyObjectiveAdvisor
    {
        public static DailyObjective Build(int food, int companions, bool villageHasHeat, bool isNight)
        {
            if (!villageHasHeat) return new DailyObjective { Goal = "Restore village heat", Risk = "Cold damage in the safe zone", FirstAction = "Gather wood and light a campfire" };

            // Immediate survival must win over next-day preparation. Otherwise a food
            // shortage can hide the active night wave when the player needs direction most.
            if (isNight) return new DailyObjective { Goal = "Survive the night", Risk = "Zombie wave active", FirstAction = "Return to the barricade and protect companions" };
            if (food < companions) return new DailyObjective { Goal = "Secure tomorrow's food", Risk = $"Food shortage: {companions - food}", FirstAction = "Hunt or gather food before exploring farther" };
            return new DailyObjective { Goal = "Explore one outer chunk", Risk = "Distance from village heat", FirstAction = "Mark a return route before gathering" };
        }
    }
}
