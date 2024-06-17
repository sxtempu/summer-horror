using UnityEngine;

namespace HorrorEngine
{
    public class CutsceneUtils : MonoBehaviour
    {
        public void PauseGame()
        {
            PauseController.Instance.Pause();
        }

        public void ResumeGame()
        {
            PauseController.Instance.Resume();
        }

        public void SetPlayerEnabled(bool enabled)
        {
            if (!enabled)
                GameManager.Instance.Player.Disable(this);
            else
                GameManager.Instance.Player.Enable(this);
        }

        public void SetPlayerVisible(bool visible)
        {
            GameManager.Instance.Player.SetVisible(visible);
        }

    }
}