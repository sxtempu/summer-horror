using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HorrorEngine
{
    public class CameraSystem : SingletonBehaviour<CameraSystem>
    {
        public static readonly int CamPreviewOverride = -1;

        private CinemachineBrain m_Brain;
        private Camera m_MainCamera;

        public CinemachineVirtualCamera ActiveCamera => m_Brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        public Camera MainCamera => m_MainCamera;

        // --------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            m_Brain = GetComponentInChildren<CinemachineBrain>();
            m_Brain.ReleaseCameraOverride(CamPreviewOverride);

            m_MainCamera = m_Brain.GetComponent<Camera>();

            // Enable post-processing on the main camera
            if (m_MainCamera != null)
            {
                m_MainCamera.allowHDR = true;
                m_MainCamera.allowMSAA = true;
                m_MainCamera.useOcclusionCulling = true; // Optionally enable occlusion culling

                // Enable post-processing for URP
                EnableURPPostProcessing(m_MainCamera);
            }
        }

        // --------------------------------------------------------------------

        void OnDestroy()
        {
            CameraStack.Instance.ClearAllCameras();
        }

        private void EnableURPPostProcessing(Camera camera)
        {
            UniversalAdditionalCameraData cameraData = camera.gameObject.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null)
            {
                cameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            cameraData.renderPostProcessing = true;
        }
    }
}
