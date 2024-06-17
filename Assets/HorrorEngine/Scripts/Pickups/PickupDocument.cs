using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorEngine
{
    public class PickupDocument : Pickup
    {
        [FormerlySerializedAs("Data")]
        [SerializeField] private DocumentData m_Data;
        [SerializeField] private bool m_ReadOnPickup = true;

        public override void Take()
        {
            GameManager.Instance.Inventory.Documents.Add(m_Data);

            if (m_ReadOnPickup)
                Read();

            gameObject.SetActive(false);

            base.Take();
        }

        public void Read()
        {
            UIManager.Get<UIDocument>().Show(m_Data);
        }
    }
}