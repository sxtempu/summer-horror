using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace HorrorEngine
{
    public class HECore : MonoBehaviour
    {
        [SerializeField] private HECorePrefabs CorePrefabs;
        [SerializeField] private SceneReference[] DestroyInScenes;

        // --------------------------------------------------------------------

        void Awake()
        {
            Debug.Assert(transform.parent == null, "HECore has to be placed at the top level of the hierarchy (without a parent)");

            // Ensure only one HECore exists
            HECore[] cores = FindObjectsOfType<HECore>();
            if (cores.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            SceneManager.activeSceneChanged += OnSceneChange;
        }

        // --------------------------------------------------------------------

        private void Start()
        {            
            if (CorePrefabs)
            {
                InitCorePrefabs();
            }
            else
            {
                Debug.LogWarning("CorePrefabs has not been assigned. Are you using a deprecated HECommon? Use HECore prefab with a valid reference to CorePrefabs", gameObject);
            }
        }

        // --------------------------------------------------------------------

        private void InitCorePrefabs()
        {
            var mapped = CorePrefabs.GetMappedPrefabs();
            foreach(var mapEntry in mapped)
            {
                string path = mapEntry.Key;
                List<GameObject> objects = mapEntry.Value;

                Transform parent = string.IsNullOrEmpty(path) ? transform : transform.Find(path);
                if (!parent)
                {
                    GameObject parentGO = new GameObject(path);
                    parent = parentGO.transform;
                    parent.SetParent(transform);
                }

                foreach(var go in objects)
                {
                    if (!go)
                        continue;

                    Instantiate(go, parent);
                }
            }
        }

        // --------------------------------------------------------------------

        void OnSceneChange(Scene oldScene, Scene newScene)
        {
            var oldEventSystem = EventSystem.current;
            var selected = oldEventSystem?.currentSelectedGameObject;

            bool destroyed = false;
            foreach (var scene in DestroyInScenes)
            {
                if (newScene.name == scene.name)
                {
                    SceneManager.activeSceneChanged -= OnSceneChange;
                    Destroy(this.gameObject);
                    destroyed = true;
                }
            }

            //This fixes the issue where a new EventSystem doesn't work after the existing one is destroyed
            var newEventSystem = EventSystem.current;
            if (destroyed && oldEventSystem != newEventSystem && newEventSystem)
            {
                newEventSystem.gameObject.SetActive(false);
                newEventSystem.gameObject.SetActive(true);

                if (selected)
                    newEventSystem.SetSelectedGameObject(selected);
            }
        }
    }
}