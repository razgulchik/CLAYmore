namespace CLAYmore.ECS
{
    public class PlayerStatsComponent : IComponent
    {
        public int   DamageBonus       = 0;
        public int   GoldBonusPerPot   = 0;
        public float PriceDiscount     = 0f;
        public float LandDiscount      = 0f;
        public float BaseMoveTime           = 0.4f;
        public float BaseBounceReturnTime   = 0.4f;
        public float SpeedMultiplier        = 1f;
        public bool  HasDash           = false;
        public bool  HasWhirlwind      = false;
        public int   WhirlwindRadius   = 1;
        public int   WhirlwindDamage   = 1;
        public bool  HasFireBlaze    = false;
        public int   FireBlazeDamage = 1;
        public int   ShieldCurrent     = 0;
        public int   ShieldMax         = 0;
        public float ShieldCooldown    = 0f;
        public float ShieldCooldownMax = 10f;
        public bool  HasLightning      = false;
        public int   LightningDamage   = 1;
        public float LightningInterval = 0f;
        public float LightningTimer    = 0f;
        public int   LongSwordReach    = 0;
        public float GoldenUrnChance    = 0f;
        public int   GoldenUrnGoldReward = 10;
        public float HearthChance        = 0f;
        public bool  HasShockwave           = false;
        public int   ShockwaveDamage        = 1;
        public int   ShockwaveStepCount     = 0;
        public int   ShockwaveStepsRequired = 3;
        public bool  HasFireTrail        = false;
        public int   FireTrailDamage     = 1;
        public bool  HasBallLightning    = false;
        public int   BallLightningDamage = 1;
        public int   BallLightningRadius = 1;
    }
}
