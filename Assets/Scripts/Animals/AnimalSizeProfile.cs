namespace IL6
{
    public readonly struct AnimalSizeProfile
    {
        // Size is opaque side-view height; hawk uses maximum wingspan across its flight frames.
        public readonly float Size, ActorScale, FootprintRadius;
        private AnimalSizeProfile(float size, float actorScale, float footprint)
        { Size=size; ActorScale=actorScale; FootprintRadius=footprint; }
        public static AnimalSizeProfile For(string key) => key switch
        {
            "rabbit" => new AnimalSizeProfile(.40f,.75f,.12f),
            "snow_hare" => new AnimalSizeProfile(.46f,.7f,.14f),
            "fox" => new AnimalSizeProfile(.60f,1.05f,.23f),
            "dog" => new AnimalSizeProfile(.70f,.55f,.26f),
            "wolf" => new AnimalSizeProfile(.95f,1.3f,.32f),
            "boar" => new AnimalSizeProfile(1.0f,1.9f,.40f),
            "deer" => new AnimalSizeProfile(2.05f,1.5f,.46f),
            "bear" => new AnimalSizeProfile(1.60f,2.25f,.65f),
            "mammoth" => new AnimalSizeProfile(3.40f,9.9f,1.15f),
            "hawk" => new AnimalSizeProfile(1.10f,.45f,.16f),
            _ => throw new System.ArgumentOutOfRangeException(nameof(key),key,"Unknown animal species")
        };
    }
}
