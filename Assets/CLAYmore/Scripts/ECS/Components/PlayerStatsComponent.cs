namespace CLAYmore.ECS
{
    public class PlayerStatsComponent : IComponent
    {
        public int   DamageBonus       = 0;
        public float BaseMoveTime           = 0.4f;
        public float BaseBounceReturnTime   = 0.4f;
        public float SpeedMultiplier        = 1f;
        public bool  HasDash           = false;
        public bool  HasWhirlwind      = false;
        public int   WhirlwindRadius   = 1;
        public int   WhirlwindDamage   = 1;
        public bool  HasFireBalls    = false;
        public int   FireBallsDamage = 1;
        public int   ShieldCurrent     = 0;
        public int   ShieldMax         = 0;
        public float ShieldCooldown    = 0f;
        public float ShieldCooldownMax = 10f;
        public bool  HasLightning      = false;
        public int   LightningDamage   = 1;
        public float LightningInterval = 0f;
        public float LightningTimer    = 0f;
    }
}
