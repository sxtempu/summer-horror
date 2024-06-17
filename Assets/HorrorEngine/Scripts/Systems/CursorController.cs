using UnityEngine;

namespace HorrorEngine
{
    public class CursorController : SingletonBehaviour<CursorController>
    {
        public bool Lock;
        public bool Visible;
        public bool UnlockForUI = true;
        public bool VisibleForUI = true;

        private int m_InUICount;
        protected override void Awake()
        {
            base.Awake();

            m_InUICount = 1;
            SetInUI(false);
        }

        public void SetInUI(bool inUI)
        {
            if (!inUI)
            {
                --m_InUICount;
                Debug.Assert(m_InUICount >= 0, "Cursor InUI count went negative. This shouldn't happen. Ssomething calling SetInUI multiple times with the same value");

                if (m_InUICount == 0)
                {
                    Cursor.lockState = Lock ? CursorLockMode.Locked : CursorLockMode.None;
                    Cursor.visible = Visible;
                }
            }
            else
            {
                Cursor.lockState = UnlockForUI ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = VisibleForUI;
                ++m_InUICount;
            }
        }


    }
}
