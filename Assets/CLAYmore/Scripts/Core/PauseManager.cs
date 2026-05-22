using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class PauseManager
    {
        public static readonly PauseManager Instance = new();

        private int  _count;
        private bool _userPaused;

        public void Reset()
        {
            _count      = 0;
            _userPaused = false;
            Time.timeScale = 1f;
        }

        public void ToggleUserPause()
        {
            if (_userPaused)
            {
                _userPaused = false;
                Pop();
            }
            else
            {
                _userPaused = true;
                Push();
            }
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
