using UnityEngine;

namespace HorrorEngine
{
    public class GamePausedMessage : BaseMessage
    {
        public static GamePausedMessage Default = new GamePausedMessage();
    }

    public class GameUnpausedMessage : BaseMessage
    {
        public static GameUnpausedMessage Default = new GameUnpausedMessage();
    }

    public class PauseController : SingletonBehaviourDontDestroy<PauseController>
    {
        private int mPauseCount;

        public bool IsPaused => mPauseCount > 0;

        public void Pause()
        {
            ++mPauseCount;
            if (mPauseCount == 1)
                MessageBuffer<GamePausedMessage>.Dispatch(GamePausedMessage.Default);

            Time.timeScale = 0f;
        }

        // --------------------------------------------------------------------

        public void Resume()
        {
            --mPauseCount;

            if (mPauseCount <= 0)
            {
                MessageBuffer<GameUnpausedMessage>.Dispatch(GameUnpausedMessage.Default);
                Time.timeScale = 1f;
            }

            Debug.Assert(mPauseCount >= 0, "PauseController:  PauseCount went below 0");
        }

#if UNITY_EDITOR
        public void Update()
        {
            if (Input.GetKey(KeyCode.Numlock))
            {
                Debug.Break();
            }
        }
#endif
    }

    
}