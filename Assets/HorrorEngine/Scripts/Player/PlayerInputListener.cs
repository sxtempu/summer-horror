using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HorrorEngine
{
#if ENABLE_INPUT_SYSTEM
    public class InputActionProcessor
    {
        public int FrameDown;
        public int FrameUp;
        
        public bool IsDown() { return FrameDown == Time.frameCount; }
        public bool IsUp() { return FrameUp == Time.frameCount; }
        public bool IsHeld() { return FrameDown != -1 && FrameUp == -1; }

        public void Process(InputValue value)
        {
            if (IsHeld() && !value.isPressed)
            {
                FrameDown = -1;
                FrameUp = Time.frameCount;
            }
            else if (value.isPressed)
            {
                FrameUp = -1;
                FrameDown = Time.frameCount;
            }
        }

        public void Clear()
        {
            FrameDown = -1;
            FrameUp = -1;
        }
    }

    public class PlayerInputListener : MonoBehaviour, IPlayerInput
    {
        private Vector2 m_InputAxis;
        private InputActionProcessor m_AimingP = new InputActionProcessor();
        private InputActionProcessor m_AttackP = new InputActionProcessor();
        private InputActionProcessor m_InteractP = new InputActionProcessor();
        private InputActionProcessor m_RunP = new InputActionProcessor();
        private InputActionProcessor m_ReloadP = new InputActionProcessor();
        private InputActionProcessor m_Turn180P = new InputActionProcessor();
        private InputActionProcessor m_ChangeAimTargetP = new InputActionProcessor();

        private void Awake()
        {
            MessageBuffer<GameUnpausedMessage>.Subscribe(OnGameUnpaused);
        }

        void OnGameUnpaused(GameUnpausedMessage msg)
        {
            Flush();
        }

        // ------------------------------------ SendMessages from PlayerInput component

        private void OnPrimaryAxis(InputValue value)
        {
            m_InputAxis = value.Get<Vector2>();
        }

        private void OnAiming(InputValue value)
        {
            m_AimingP.Process(value);
        }

        private void OnAttack(InputValue value)
        {
            m_AttackP.Process(value);
        }
        public void OnInteract(InputValue value)
        {
            m_InteractP.Process(value);
        }
        public void OnRun(InputValue value)
        {
            m_RunP.Process(value);
        }
        public void OnReload(InputValue value)
        {
            m_ReloadP.Process(value);
        }

        public void OnTurn180(InputValue value)
        {
            m_Turn180P.Process(value);
        }

        public void OnChangeAimTarget(InputValue value)
        {
            m_ChangeAimTargetP.Process(value);
        }

        // ------------------------------------ IPlayerInput implementation

        public Vector2 GetPrimaryAxis()
        {
            return m_InputAxis;
        }

        public bool IsAimingHeld()
        {
            return m_AimingP.IsHeld();
        }

        public bool IsAttackDown()
        {
            return m_AttackP.IsDown();
        }

        public bool IsAttackUp()
        {
            return m_AttackP.IsUp();
        }

        public bool IsInteractingDown()
        {
            return m_InteractP.IsDown();
        }

        public bool IsRunHeld()
        {
            return m_RunP.IsHeld();
        }

        public bool IsReloadDown()
        {
            return m_ReloadP.IsDown();
        }

        public bool IsTurn180Down()
        {
            return m_Turn180P.IsDown();
        }


        public bool IsChangeAimTargetDown()
        {
            return m_ChangeAimTargetP.IsDown();
        }

        public void Flush()
        {
            m_AimingP.Clear();
            m_AttackP.Clear();
            m_InteractP.Clear();
            m_RunP.Clear();
            m_ReloadP.Clear();
            m_Turn180P.Clear();
            m_ChangeAimTargetP.Clear();
        }
    }
#else
    public class PlayerInputNew : MonoBehaviour { }
#endif
}

