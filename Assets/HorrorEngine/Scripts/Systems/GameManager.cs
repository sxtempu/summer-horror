using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HorrorEngine
{
    [Serializable]
    public class GameSaveData
    {
        public static readonly int k_CurrentVersion = 1; // Increase this number to force player save data clearance
        
        public int Version;
        public string Date;
        public string PlayerName;
        public int SaveCount;
        public string SaveLocation;
        public string SceneName;
        public InventorySaveData Inventory;
        public ContainerSaveData StorageBox;
    }

    public class GameOverMessage : BaseMessage { }

    public class GameManager : SingletonBehaviour<GameManager>, ISavable<GameSaveData>
    {
        public string PlayerName = "PlayerName";
        public PlayerActor Player;
        public Inventory Inventory;
        public ContainerData StorageBox;

        [Header("Databases")]
        public ItemDatabase ItemDatabase;
        public DocumentDatabase DocumentDatabase;
        public MapDatabase MapDatabase;
        public SpawnableSavableDatabase SpawnableDatabase;

        [Header("Initial Game State")]
        public InventoryEntry[] InitialInventory;
        public DocumentData[] InitialDocuments;
        public MapData[] InitialMaps;
        public EquipableItemData[] InitialEquipment;

        [HideInInspector]
        public UnityEvent<PlayerActor> OnPlayerRegistered;

        private bool m_IsPlaying;

        // --------------------------------------------------------------------

        public bool IsPlaying { 
            get
            {
                return !PauseController.Instance.IsPaused && m_IsPlaying;
            }
            set
            {
                m_IsPlaying = value;
            }
        }

        // --------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            InitializeRegisters();

            Inventory.Init();

            StorageBox.FillCapacityWithEmptyEntries();

            MessageBuffer<DoorCrossSceneTransitionBeforeSpawnMessage>.Subscribe(OnDoorCrossSceneTransitionBeforeSpawn);
            MessageBuffer<DoorTransitionMidWayMessage>.Subscribe(OnDoorTransitionMidway);
        }
        // --------------------------------------------------------------------

        private void OnDestroy()
        {
            MessageBuffer<DoorCrossSceneTransitionBeforeSpawnMessage>.Unsubscribe(OnDoorCrossSceneTransitionBeforeSpawn);
            MessageBuffer<DoorTransitionMidWayMessage>.Unsubscribe(OnDoorTransitionMidway);
        }

        // --------------------------------------------------------------------

        public void RegisterPlayer(PlayerActor player, bool teleportToSpawnPoint = true)
        {
            if (Player)
            {
                Player.GetComponent<Health>().OnDeath.RemoveListener(OnPlayerDeath);
            }

            Player = player;
            Player.GetComponent<Health>().OnDeath.AddListener(OnPlayerDeath);

            SetupInitialEquipment(player);

            if (teleportToSpawnPoint)
            {
                PlayerSpawnPoint spawnPoint = FindObjectOfType<PlayerSpawnPoint>();
                if (spawnPoint)
                {
                    player.transform.position = spawnPoint.transform.position;
                    player.transform.LookAt(spawnPoint.transform.position + spawnPoint.transform.forward * 10, Vector3.up);
                    player.GetComponent<GameObjectReset>().ResetComponents(); // This is to reset the animSpeedSetter, but might be overkill
                }
            }

            OnPlayerRegistered?.Invoke(player);
        }

        // --------------------------------------------------------------------

        private void SetupInitialEquipment(PlayerActor player)
        {
            PlayerEquipment equipment = Player.GetComponentInChildren<PlayerEquipment>();
            foreach (var e in InitialEquipment)
            {
                if (Inventory.TryGet(e, out InventoryEntry entry))
                    Inventory.Equip(entry);
                else
                    equipment.Equip(e, e.Slot);
            }
        }

        // --------------------------------------------------------------------

        private void Start()
        {
            m_IsPlaying = true;
        }

        // --------------------------------------------------------------------

        public void InitializeRegisters()
        {
            ItemDatabase.HashRegisters();
            DocumentDatabase.HashRegisters();
            MapDatabase.HashRegisters();
        }

        // --------------------------------------------------------------------

        void OnDoorTransitionMidway(DoorTransitionMidWayMessage msg)
        {
            ObjectStateManager.Instance.CaptureStates();
        }

        // --------------------------------------------------------------------

        void OnDoorCrossSceneTransitionBeforeSpawn(DoorCrossSceneTransitionBeforeSpawnMessage msg)
        {
            ObjectStateManager.Instance.InstantiateSpawned(SceneManager.GetActiveScene(), SpawnableDatabase);
            ObjectStateManager.Instance.ApplyStates();
        }

        // --------------------------------------------------------------------

        void OnPlayerDeath(Health health)
        {
            MessageBuffer<GameOverMessage>.Dispatch();
            IsPlaying = false;   
        }

        //------------------------------------------------------
        // ISavable implementation
        //------------------------------------------------------

        public GameSaveData GetSavableData()
        {
            GameSaveData savedData = new GameSaveData();
            savedData.Version = GameSaveData.k_CurrentVersion;
            savedData.Date = DateTime.Now.ToString();
            savedData.PlayerName = PlayerName;
            savedData.SceneName = SceneManager.GetActiveScene().name;
            savedData.Inventory = Inventory.GetSavableData();
            savedData.StorageBox = StorageBox.GetSavableData();

            return savedData;
        }

        public void SetFromSavedData(GameSaveData savedData)
        {
            PlayerName = savedData.PlayerName;
            Inventory.SetFromSavedData(savedData.Inventory);
            StorageBox.SetFromSavedData(savedData.StorageBox);
        }


    }
}