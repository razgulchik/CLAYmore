namespace CLAYmore.ECS
{
    public class PlayerStatsComponent : IComponent
    {
        public int   DamageBonus       = 0;
        public bool  HasDash           = false;
        public bool  HasAoeStrike      = false;
        public bool  HasOrthoStrike    = false;
        public int   ShieldCurrent     = 0;
        public int   ShieldMax         = 0;
        public float ShieldCooldown    = 0f;
        public float ShieldCooldownMax = 10f;
        public bool  HasLightning      = false;
        public float LightningInterval = 0f;
        public float LightningTimer    = 0f;
    }
}
