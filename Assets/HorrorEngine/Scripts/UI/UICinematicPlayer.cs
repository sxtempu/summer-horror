using UnityEngine;
using UnityEngine.Video;

namespace HorrorEngine
{
    public class UICinematicPlayer : MonoBehaviour
    {
        private VideoPlayer m_Player;

        private void Awake()
        {
            m_Player = GetComponentInChildren<VideoPlayer>();
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void Show(VideoClip clip)
        {
            PauseController.Instance.Pause();
            gameObject.SetActive(true);

            m_Player.clip = clip;
            m_Player.Play();

            this.InvokeActionUnscaled(Hide, (float)clip.length);
        }


        private void Hide()
        {
            PauseController.Instance.Resume();
            gameObject.SetActive(false);
            UIManager.PopAction();
        }


    }
}