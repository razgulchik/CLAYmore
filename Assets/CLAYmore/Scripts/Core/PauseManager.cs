using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class PauseManager
    {
        public static readonly PauseManager Instance = new();

        private int _count;

        public void Reset()
        {
            _count = 0;
            Time.timeScale = 1f;
        }

        public void Push()
        {
            _count++;
            if (_count == 1)
                ApplyPause(true);
        }

        public void Pop()
        {
            if (_count == 0) return;
            _count--;
            if (_count == 0)
                ApplyPause(false);
        }

        private void ApplyPause(bool paused)
        {
            Time.timeScale = paused ? 0f : 1f;
            World.Current?.Events.Publish(new GamePausedEvent { IsPaused = paused });
        }
    }
}
