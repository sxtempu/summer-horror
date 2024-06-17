using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HorrorEngine
{
    public class UIMainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject m_LoadSlotsScreen;
        [SerializeField] private Button m_NewButton;
        [SerializeField] private Button m_LoadButton;
        [SerializeField] private Button m_QuitButton;
        [SerializeField] private AudioClip m_CloseSlotsClip;
        [FormerlySerializedAs("StartScene")]
        [SerializeField] private SceneReference m_StartScene;

        private IUIInput m_Input;
        private UISaveGameList m_SlotList;
        
        private void Awake()
        {
            m_Input = GetComponentInParent<IUIInput>();

            m_SlotList = m_LoadSlotsScreen.GetComponentInChildren<UISaveGameList>();
            m_SlotList.OnSubmit.AddListener(OnLoadSlotSubmit);
        }

        private void OnLoadSlotSubmit(GameObject selected)
        {
            int slotIndex = m_SlotList.GetSelectedIndex();
            
            SaveDataManager<GameSaveData> saveMgr = SaveDataManager<GameSaveData>.Instance;
            bool slotExists = saveMgr.SlotExists(slotIndex);

            if (slotExists)
            {
                gameObject.SetActive(false);
                GameSaveUtils.LoadSlot(slotIndex);
            }
            else
            {
                CloseSlotsScreen();
            }
        }


        private void Start()
        {
            int lastSavedSlot = GameSaveUtils.GetLastSavedSlot();
            m_LoadButton.gameObject.SetActive(lastSavedSlot >= 0);
            m_LoadSlotsScreen.SetActive(false);
        }

        private void Update()
        {
            if (m_LoadSlotsScreen.activeSelf)
            {
                if (m_Input.IsConfirmDown())
                {
                    OnLoadSlotSubmit(m_SlotList.GetSelected());
                }
                else if (m_Input.IsCancelDown())
                {
                    CloseSlotsScreen();
                }
            }

            if (EventSystem.current.currentSelectedGameObject == null) // Give back focus to buttons if lost
                SelectDefault();
        }

        private void CloseSlotsScreen()
        {
            m_NewButton.gameObject.SetActive(true);
            m_LoadButton.gameObject.SetActive(true);
            m_QuitButton.gameObject.SetActive(true);
            m_LoadSlotsScreen.SetActive(false);
            SelectDefault();

            if (m_CloseSlotsClip)
                UIManager.Get<UIAudio>().Play(m_CloseSlotsClip);
        }

        private void SelectDefault()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(m_NewButton.gameObject);
        }

        public void NewGame()
        {
            gameObject.SetActive(false);

            GameSaveUtils.StartGame(m_StartScene);
        }

        public void LoadGame()
        {
            m_Input.Flush(); // Prevents selecting the first slot immediately

            m_NewButton.gameObject.SetActive(false);
            m_LoadButton.gameObject.SetActive(false);
            m_QuitButton.gameObject.SetActive(false);

            m_LoadSlotsScreen.gameObject.SetActive(true);
            m_SlotList.FillSlotsInfo();
            m_SlotList.SelectDefault();
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.ExecuteMenuItem("Edit/Play");
#else
            Application.Quit();
#endif
        }
    }

}