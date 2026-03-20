namespace CLAYmore.ECS
{
    /// <summary>
    /// Interface for all ECS systems.
    /// Implementations must contain ONLY logic — no mutable state (except caches).
    /// </summary>
    public interface ISystem
    {
        void Initialize(World world);
        void Tick(float deltaTime);
    }
}
