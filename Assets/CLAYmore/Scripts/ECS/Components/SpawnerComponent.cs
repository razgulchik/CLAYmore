using CLAYmore.ECS;

namespace CLAYmore
{
    public class SpawnerComponent : IComponent
    {
        public float InitialInterval;
        public float MinInterval;
        public float IntervalDecreasePerSecond;
        public float CurrentInterval;
        public float Timer;
    }
}
