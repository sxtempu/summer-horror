using System;
using UnityEngine;
using UnityEngine.AI;

namespace HorrorEngine
{
    public interface IPlayerMovementSettings
    {
        float GetFwdRate(PlayerMovement movement);
        float GetRightRate(PlayerMovement movement);
        void GetRotation(PlayerMovement movement, out float sign, out float rate);
    }

    public class PlayerMovement : MonoBehaviour, IDeactivateWithActor
    {
        [Flags]
        public enum MovementConstrain
        {
            None = 0,
            Movement = 1,
            Rotation = 2
        }

        [HideInInspector]
        public MovementConstrain Constrain = 0;

        [Header("Main settings")]
        [SerializeField] float m_MovementSpeed;
        [SerializeField] float m_MovementRunSpeed;
        [SerializeField] float m_MovementBackwardsSpeed;
        [SerializeField] float m_MovementLateralSpeed;
        [SerializeField] float m_NavMeshCheckDistance;
        [SerializeField] bool m_AnalogRunning;

        [Header("Health Modifiers")]
        [SerializeField] bool m_ChangeWalkSpeedBasedOnHealth;
        [SerializeField] AnimationCurve m_NormalizedHealthSpeedScalar = AnimationCurve.Linear(0, 1f, 1f, 1f);
        [SerializeField] bool m_ChangeRunSpeedBasedOnHealth;
        [SerializeField] AnimationCurve m_NormalizedHealthRunSpeedScalar = AnimationCurve.Linear(0, 1f, 1f, 1f);

        private Rigidbody m_Rigidbody;
        private Vector2 m_InputAxis;
        private GroundDetector m_GroundDetector;
        private IPlayerInput m_Input;
        private bool m_Running;
        private Health m_Health;
        private IPlayerMovementSettings m_Settings;

        public Vector3 IntendedMovement { get; private set; }
        public Vector2 InputAxis => m_InputAxis;

        private void Awake()
        {
            m_Input = GetComponent<IPlayerInput>();
            m_Health = GetComponent<Health>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_GroundDetector = GetComponent<GroundDetector>();
            m_Settings = GetComponent<IPlayerMovementSettings>();

            Debug.Assert(m_Settings != null, "Character doesn't have any movement settings component", gameObject);
        }

        private void Update()
        {
            if (!m_AnalogRunning)
                m_Running = m_Input.IsRunHeld();

            m_InputAxis = m_Input.GetPrimaryAxis();

            // Check for lock-on input
            if (m_Input.IsLockOnDown())
            {
                // Implement lock-on behavior
                HandleLockOn();
            }
        }

        private void FixedUpdate()
        {
            if (!Constrain.HasFlag(MovementConstrain.Movement))
                UpdateMovement();

            if (!Constrain.HasFlag(MovementConstrain.Rotation))
                UpdateRotation();
        }

        private void UpdateRotation()
        {
            m_Settings.GetRotation(this, out float sign, out float rate);
            if (rate > 0)
                Rotate(sign, rate);
        }

        public void Rotate(float dir, float speed)
        {
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * Quaternion.Euler(Vector3.up * dir * Time.deltaTime * speed));
        }

        private void UpdateMovement()
        {
            Vector3 movement = Vector3.zero;
            movement += GetForwardMovement(out float absFwd);
            movement += GetRightMovement();

            IntendedMovement = movement;

            if (movement != Vector3.zero)
            {
                float speed = movement.magnitude;

                Vector3 prevPos = m_Rigidbody.position;
                Vector3 newPos = prevPos + IntendedMovement;

                // Correct the movement to check the ground slope
                if (m_GroundDetector.Detect(newPos))
                {
                    Vector3 dir = (m_GroundDetector.Position - prevPos).normalized;
                    Vector3 actualMovement = dir * speed;
                    newPos = prevPos + actualMovement;
                }

                // Navmesh sliding
                if (NavMesh.SamplePosition(newPos, out NavMeshHit hit, m_NavMeshCheckDistance, NavMesh.AllAreas))
                {
                    Vector3 navMeshNewPos;
                    if (m_GroundDetector.Detect(hit.position))
                    {
                        navMeshNewPos = m_GroundDetector.Position;
                    }
                    else
                    {
                        navMeshNewPos = hit.position;
                    }

                    //Check sliding direction
                    Vector3 dirToNewPos = (newPos - prevPos).normalized;
                    Vector3 dirToNavMeshNewPos = (navMeshNewPos - prevPos).normalized;
                    float dot = Vector3.Dot(dirToNewPos, dirToNavMeshNewPos);
                    if (dot > 0)
                    {
                        newPos = prevPos + dirToNavMeshNewPos * Vector3.Distance(newPos, prevPos) * dot; // Scale using the dot to reduce speed at a perpendicular angle
                    }
                    else
                    {
                        newPos = prevPos; // No sliding since it was going to be a backward movement
                    }

                }

                m_Rigidbody.MovePosition(newPos);
            }
        }

        Vector3 GetForwardMovement(out float absFwd)
        {
            float fwd = m_Settings.GetFwdRate(this);
            absFwd = Mathf.Abs(fwd);

            float speed = 0f;
            if (m_AnalogRunning)
            {
                if (fwd > Mathf.Epsilon)
                    speed = Mathf.Lerp(m_MovementSpeed * absFwd, m_MovementRunSpeed, absFwd);
                else if (fwd < -Mathf.Epsilon)
                    speed = m_MovementBackwardsSpeed * absFwd;
            }
            else
            {
                if (fwd > Mathf.Epsilon)
                    speed = m_Running ? m_MovementRunSpeed : m_MovementSpeed;
                else if (fwd < -Mathf.Epsilon)
                    speed = m_MovementBackwardsSpeed;
            }

            if (speed >= m_MovementRunSpeed && m_ChangeRunSpeedBasedOnHealth)
                speed *= m_NormalizedHealthRunSpeedScalar.Evaluate(m_Health.Normalized);
            else if (m_ChangeRunSpeedBasedOnHealth)
                speed *= m_NormalizedHealthSpeedScalar.Evaluate(m_Health.Normalized);

            return transform.forward * Time.deltaTime * speed * Mathf.Sign(fwd);
        }

        Vector3 GetRightMovement()
        {
            float right = m_Settings.GetRightRate(this);
            float absRight = Mathf.Abs(right);

            float speed = 0f;
            if (m_AnalogRunning)
            {
                if (right > Mathf.Epsilon || right < -Mathf.Epsilon)
                    speed = m_MovementLateralSpeed * absRight;
            }
            else
            {
                if (right > Mathf.Epsilon || right < -Mathf.Epsilon)
                    speed = m_MovementLateralSpeed;
            }

            if (speed >= m_MovementRunSpeed && m_ChangeRunSpeedBasedOnHealth)
                speed *= m_NormalizedHealthRunSpeedScalar.Evaluate(m_Health.Normalized);
            else if (m_ChangeRunSpeedBasedOnHealth)
                speed *= m_NormalizedHealthSpeedScalar.Evaluate(m_Health.Normalized);

            return transform.right * Time.deltaTime * speed * Mathf.Sign(right);
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position - transform.right, transform.position + transform.right);
            Gizmos.DrawLine(transform.position - transform.forward, transform.position + transform.forward);
        }

        public void AddConstrain(MovementConstrain constrain) { Constrain |= constrain; }
        public void RemoveConstrain(MovementConstrain constrain) { Constrain &= ~constrain; }

        // Handle lock-on behavior
        private void HandleLockOn()
        {
            // Find the PlayerStateAiming component
            var aimingState = GetComponentInParent<PlayerStateAiming>();
            if (aimingState != null)
            {
                // Toggle lock-on state
                aimingState.ToggleLockOn();
            }
        }

        // Constrain movement to strafing
        public void ConstrainToStrafe()
        {
            // Implement strafing logic here
            // For example, limit the player's movement to only left and right directions
            Vector3 rightMovement = GetRightMovement();
            IntendedMovement = rightMovement;

            if (rightMovement != Vector3.zero)
            {
                float speed = rightMovement.magnitude;
                Vector3 prevPos = m_Rigidbody.position;
                Vector3 newPos = prevPos + rightMovement;

                // Correct the movement to check the ground slope
                if (m_GroundDetector.Detect(newPos))
                {
                    Vector3 dir = (m_GroundDetector.Position - prevPos).normalized;
                    Vector3 actualMovement = dir * speed;
                    newPos = prevPos + actualMovement;
                }

                // Navmesh sliding
                if (NavMesh.SamplePosition(newPos, out NavMeshHit hit, m_NavMeshCheckDistance, NavMesh.AllAreas))
                {
                    Vector3 navMeshNewPos;
                    if (m_GroundDetector.Detect(hit.position))
                    {
                        navMeshNewPos = m_GroundDetector.Position;
                    }
                    else
                    {
                        navMeshNewPos = hit.position;
                    }

                    Vector3 dirToNewPos = (newPos - prevPos).normalized;
                    Vector3 dirToNavMeshNewPos = (navMeshNewPos - prevPos).normalized;
                    float dot = Vector3.Dot(dirToNewPos, dirToNavMeshNewPos);
                    if (dot > 0)
                    {
                        newPos = prevPos + dirToNavMeshNewPos * Vector3.Distance(newPos, prevPos) * dot;
                    }
                    else
                    {
                        newPos = prevPos;
                    }
                }

                m_Rigidbody.MovePosition(newPos);
            }
        }
    }
}
