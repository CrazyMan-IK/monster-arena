using System;

namespace MonsterArena
{
    public class Timer
    {
        public event Action Ticked = null;

        private readonly float _duration = 0;
        private float _time = 0;

        public Timer(float duration)
        {
            if (duration <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            _duration = duration;
        }

        public void Update(float deltaTime)
        {
            _time += deltaTime;

            if (_time >= _duration)
            {
                _time = 0;

                Ticked?.Invoke();
            }
        }
    }
}
