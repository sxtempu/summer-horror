using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HorrorEngine
{
#if ENABLE_INPUT_SYSTEM
    public class UIInputNew : MonoBehaviour, IUIInput
    {
        private Vector2 m_InputAxis;

        private InputActionProcessor m_CancelP = new InputActionProcessor();
        private InputActionProcessor m_ConfirmP = new InputActionProcessor();
        private InputActionProcessor m_ToggleInventoryP = new InputActionProcessor();
        private InputActionProcessor m_ToggleMapP = new InputActionProcessor();
        private InputActionProcessor m_ToggleMapListP = new InputActionProcessor();
        private InputActionProcessor m_TogglePauseP = new InputActionProcessor();
        private InputActionProcessor m_NextSubmapP = new InputActionProcessor();
        private InputActionProcessor m_PrevSubmapP = new InputActionProcessor();

        // ------------------------------------ SendMessages from PlayerInput component

        private void OnCancel(InputValue value)
        {
            m_CancelP.Process(value);
        }

        private void OnConfirm(InputValue value)
        {
            m_ConfirmP.Process(value);
        }

        private void OnToggleInventory(InputValue value)
        {
            m_ToggleInventoryP.Process(value);
        }

        private void OnToggleMap(InputValue value)
        {
            m_ToggleMapP.Process(value);
        }
        private void OnToggleMapList(InputValue value)
        {
            m_ToggleMapListP.Process(value);
        }

        private void OnTogglePause(InputValue value)
        {
            m_TogglePauseP.Process(value);
        }

        private void OnNextSubmap(InputValue value)
        {
            m_NextSubmapP.Process(value);
        }

        private void OnPrevSubmap(InputValue value)
        {
            m_PrevSubmapP.Process(value);
        }


        private void OnPrimaryAxis(InputValue value)
        {
            m_InputAxis = value.Get<Vector2>();
        }

        // ------------------------------------- IUIInput Implementation

        public bool IsCancelDown()
        {
            return m_CancelP.IsDown();
        }
        public bool IsConfirmDown()
        {
            return m_ConfirmP.IsDown();
        }

        public bool IsTogglePauseDown()
        {
            return m_TogglePauseP.IsDown();
        }

        public bool IsToggleInventoryDown()
        {
            return m_ToggleInventoryP.IsDown();
        }

        public bool IsToggleMapDown()
        {
            return m_ToggleMapP.IsDown();
        }

        public bool IsToggleMapListDown()
        {
            return m_ToggleMapListP.IsDown();
        }

        public Vector2 GetPrimaryAxis()
        {
            return m_InputAxis;
        }

        public bool IsPrevSubmapDown()
        {
            return m_PrevSubmapP.IsDown();
        }

        public bool IsNextSubmapDown()
        {
            return m_NextSubmapP.IsDown();
        }

        public void Flush()
        {
            m_CancelP.Clear();
            m_ConfirmP.Clear();
            m_ToggleInventoryP.Clear();
            m_TogglePauseP.Clear();
            m_ToggleMapP.Clear();
            m_ToggleMapListP.Clear();
        }

    }
#else
    public class UIInputNew : MonoBehaviour { }
#endif
}