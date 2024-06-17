using UnityEngine;

namespace HorrorEngine 
{

    public class CameraPOV : MonoBehaviour
    {
        public int Priority;

        private Cinemachine.CinemachineVirtualCamera m_VirtualCam;
        private int m_TriggerEnterCount;

        private void Awake()
        {
            m_VirtualCam = GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();

            var disableNotifier = GetComponentInChildren<OnDisableNotifier>();
            if (disableNotifier) 
            {
                disableNotifier.AddCallback((notif) => 
                {
                    m_TriggerEnterCount = 0;
                    CameraStack.Instance.RemoveCamera(this);
                });
            }

            Deactivate();
        }

        private void OnTriggerEnter(Collider other)
        {
            ++m_TriggerEnterCount;
            CameraStack.Instance.AddCamera(this, Priority);
        }

        private void OnTriggerExit(Collider other)
        {
            --m_TriggerEnterCount;
            Debug.Assert(m_TriggerEnterCount >= 0, $"Trigger count went negative in CameraPOV {name}");
            if (m_TriggerEnterCount == 0)
                CameraStack.Instance.RemoveCamera(this);
        }

        public void Activate()
        {
            m_VirtualCam.gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            m_VirtualCam.gameObject.SetActive(false);
        }
    }
}