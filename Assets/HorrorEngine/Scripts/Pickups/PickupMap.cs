using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorEngine
{
    public class PickupMap : Pickup
    {
        [SerializeField] private MapData m_Data;
        [SerializeField] private string m_NameTag = "{MAPNAME}";
        [FormerlySerializedAs("m_OpenMapOnPikcup")]
        [SerializeField] private bool m_OpenMapOnPickup = true;
        [SerializeField] private bool m_GiveEntireSet = true;

        private Choice m_Choice;

        private void Awake()
        {
            m_Choice = GetComponent<Choice>();
            m_Choice.Data.ChoiceDialog.SetTagReplacement(m_NameTag, m_Data.Name);
        }

        public override void Take()
        {
            if (m_GiveEntireSet && m_Data.MapSet)
            {
                foreach(var map in m_Data.MapSet.Maps)
                    GameManager.Instance.Inventory.Maps.Add(map);
            }
            else
            {
                GameManager.Instance.Inventory.Maps.Add(m_Data);
            }

            gameObject.SetActive(false);

            if (m_OpenMapOnPickup)
                UIManager.Get<UIMap>().Show(m_Data);

            base.Take();
        }

    }
}