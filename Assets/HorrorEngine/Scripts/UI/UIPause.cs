using UnityEngine;

namespace HorrorEngine
{
    public class UIPause : MonoBehaviour
    {
        [SerializeField] private GameObject m_Content;
        [SerializeField] private GameObject m_PauseHint;
        [SerializeField] private AudioClip m_ShowClip;

        private IUIInput m_Input;

        // --------------------------------------------------------------------

        private void Awake()
        {
            m_Input = GetComponentInParent<IUIInput>();
        }

        // --------------------------------------------------------------------

        void Start()
        {
            m_Content.SetActive(false);
        }

        // --------------------------------------------------------------------

        void Update()
        {
            bool isPlaying = GameManager.Instance.IsPlaying;

            if (m_PauseHint)
                m_PauseHint.SetActive(isPlaying);

            if (isPlaying && m_Input.IsTogglePauseDown())
            {
                Show();
            }
            else if (m_Content.activeSelf && (m_Input.IsTogglePauseDown() || m_Input.IsCancelDown()))
            {
                Hide();
            }
        }

        // --------------------------------------------------------------------

        private void Show()
        {
            PauseController.Instance.Pause();
            m_Content.SetActive(true);
            UIManager.Get<UIAudio>().Play(m_ShowClip);
            CursorController.Instance.SetInUI(true);
        }

        // --------------------------------------------------------------------

        private void Hide()
        {
            PauseController.Instance.Resume();
            CursorController.Instance.SetInUI(false);
            m_Content.SetActive(false);
        }
    }
}