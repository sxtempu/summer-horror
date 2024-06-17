using UnityEngine;

namespace HorrorEngine
{
    public class FocusVirtualCameraOnCharacter : MonoBehaviour
    {
        void Start()
        {
            if (GameManager.Instance.Player)
            {
                SetPlayer(GameManager.Instance.Player.transform);
            }
            else
            {
                GameManager.Instance.OnPlayerRegistered.AddListener(OnPlayerRegistered);
            }
        }

        void SetPlayer(Transform playerT)
        {
            var cam = GetComponent<Cinemachine.CinemachineVirtualCamera>();
            cam.LookAt = playerT;
            cam.Follow = playerT;
        }

        void OnPlayerRegistered(PlayerActor player)
        {
            SetPlayer(player.transform);
            GameManager.Instance.OnPlayerRegistered.RemoveListener(OnPlayerRegistered);
        }
    }

}