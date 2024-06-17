using UnityEngine;
using System.Collections.Generic;

namespace HorrorEngine
{
    public class UIInputListener : MonoBehaviour
    {
        private IUIInput m_Input;

        private UIInventory m_Inventory;
        private UIMap m_Map;

        private HashSet<Object> m_BlockingContext = new HashSet<Object>();

        private void Update()
        {
            if (GameManager.Instance.IsPlaying)
            {
                if (m_Input == null)
                    m_Input = GetComponent<IUIInput>();

                if (m_Input != null)
                {
                    if (m_Input.IsToggleInventoryDown())
                    {
                        if (!m_Inventory)
                            m_Inventory = GetComponentInChildren<UIInventory>(true);

                        m_Inventory?.Show();
                    }

                    if (m_Input.IsToggleMapDown())
                    {
                        if (!m_Map)
                            m_Map = GetComponentInChildren<UIMap>(true);

                        m_Map?.Show();
                    }
                }
            }
        }

        // --------------------------------------------------------------------

        public void AddBlockingContext(Object context)
        {
            m_BlockingContext.Add(context);

            enabled = false;
        }

        // --------------------------------------------------------------------

        public void RemoveBlockingContext(Object context)
        {
            m_BlockingContext.Remove(context);

            if (m_BlockingContext.Count == 0)
                enabled = true;
        }
    }
}