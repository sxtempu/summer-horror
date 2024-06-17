using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HorrorEngine
{
    public class DoorCrossSceneTransitionBeforeSpawnMessage : BaseMessage { }

    public class CrossSceneDoor : DoorBase
    {
        [SerializeField] private string m_DoorUniqueId;
        [SerializeField] private SceneReference m_LoadScene;
        [SerializeField] private LoadSceneMode m_LoadMode;
        [SerializeField] private SceneReference m_UnloadScene;
        [SerializeField] private string m_ExitDoorUniqueId;

        private DoorLock m_Lock;
        private bool m_TransitionCompleted;
        private Transform m_Interactor;

        private void Awake()
        {
            m_Lock = GetComponent<DoorLock>();
        }

        // --------------------------------------------------------------------

        public override bool IsLocked()
        {
            return (m_Lock && m_Lock.IsLocked);
        }

        // --------------------------------------------------------------------

        public override void Use(IInteractor interactor)
        {
            if (m_Lock && m_Lock.IsLocked)
            {
                m_Lock.OnTryToUnlock(out bool open);
                if (!open)
                {
                    OnLocked?.Invoke();
                    return;
                }
            }

            OnOpened?.Invoke();
            MonoBehaviour interactorMB = (MonoBehaviour)interactor;
            m_Interactor = interactorMB.transform;
            DoorTransitionController.Instance.Trigger(this, interactorMB.gameObject, TransitionRoutine);
        }

        private IEnumerator TransitionRoutine()
        {
            AsyncOperation asyncUnLoad = null;
            AsyncOperation asyncLoad = null;

            if (m_LoadScene && !m_LoadScene.IsLoaded())
            {
                asyncLoad = SceneManager.LoadSceneAsync(m_LoadScene.Name, m_LoadMode);
            }

            while ((asyncLoad != null && !asyncLoad.isDone))
            {
                yield return null;
            }

            if (m_UnloadScene && m_UnloadScene.IsLoaded())
            {
                asyncUnLoad = SceneManager.UnloadSceneAsync(m_UnloadScene.Name);
            }

            while ((asyncUnLoad != null && !asyncUnLoad.isDone))
            {
                yield return null;
            }


            // This needs to happen before player teleportation or the object state will be applied on it
            MessageBuffer<DoorCrossSceneTransitionBeforeSpawnMessage>.Dispatch();

            bool doorFound = false;
            CrossSceneDoor[] doors = FindObjectsOfType<CrossSceneDoor>();
            foreach(var door in doors)
            {
                if (door.m_DoorUniqueId == m_ExitDoorUniqueId)
                {
                    m_Interactor.rotation = door.ExitPoint.rotation;
                    m_Interactor.position = door.ExitPoint.position;
                    doorFound = true;
                    break;
                }
            }

            Debug.Assert(doorFound, $"CrossScene door exit with Id: {m_ExitDoorUniqueId} not found from {doors.Length} candidates");

            yield return Yielders.UnscaledTime(1.0f);
        }

    }
}