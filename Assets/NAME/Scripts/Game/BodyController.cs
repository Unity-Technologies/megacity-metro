using UnityEngine;

namespace Unity.NAME.Game
{
    // That class is use to handle the control of a character
    // In that Microgame template, it is used for the main character but can also be used for any other characters

    [RequireComponent(typeof(CharacterController))]
    public class BodyController : MonoBehaviour
    {
        [SerializeField] LocomotionData m_LocomotionData = null;

        CharacterController m_Controller;

        Vector3 m_DesiredVelocity = Vector3.zero;
        Vector3 m_DesiredMoveDirection = Vector3.zero;
        Vector3 m_CurrentMoveDirection = Vector3.zero;
        Vector3 m_DesiredLookAtDirection = Vector3.zero;

        const float k_BigEpsilon = 0.0001f;

        public bool IsInAir { get; set; } = false;
        public bool ContinuousJump { get; set; } = false;

        private void Awake()
        {
            m_Controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            Reset();
        }

        public void Reset()
        {
            m_CurrentMoveDirection = Vector3.zero;
            m_DesiredLookAtDirection = transform.forward;
            m_DesiredMoveDirection = transform.forward;
            m_DesiredVelocity = Vector3.zero;
        }

        public void SetMovementDirection(Vector3 moveDirection) => m_DesiredMoveDirection = moveDirection;

        public bool CanJump() => !IsInAir;

        public void StartJump()
        {
            IsInAir = true;
            ContinuousJump = true;

            Vector3 flattenVelocity = m_DesiredVelocity;
            flattenVelocity.y = 0.0f;
            m_DesiredVelocity = flattenVelocity;

            m_DesiredVelocity += Vector3.up * m_LocomotionData.JumpForce;
        }

        void Update()
        {
            // Update DesiredMoveDirection and DesiredLookAtDirection
            if (m_DesiredMoveDirection.sqrMagnitude > k_BigEpsilon)
            {
                m_DesiredLookAtDirection = m_DesiredMoveDirection;
                m_DesiredLookAtDirection.Normalize();

                if (m_DesiredMoveDirection.sqrMagnitude > 1.0f)
                {
                    m_DesiredMoveDirection.Normalize();
                }
            }

            m_CurrentMoveDirection += (m_DesiredMoveDirection - m_CurrentMoveDirection)
                * Mathf.Clamp01(m_LocomotionData.MoveDirectionDampener
                * (IsInAir ? m_LocomotionData.InAirMoveDampener : 1.0f)
                * Time.deltaTime);

            // Get the full angle to match target direction
            float timeToComplete = Vector3.Angle(transform.forward, m_DesiredLookAtDirection) / m_LocomotionData.AngularSpeed;
            if (timeToComplete > 0.0f)
            {
                float percentage = Mathf.Min(1.0f, Time.deltaTime / timeToComplete);
                Vector3 newLookAtDirection = Vector3.Slerp(transform.forward, m_DesiredLookAtDirection, percentage);

                // Apply rotation
                m_Controller.transform.rotation = Quaternion.LookRotation(newLookAtDirection);
            }

            // Apply gravity Multiplier
            m_DesiredVelocity += Vector3.up * Physics.gravity.y * (IsInAir ? (m_LocomotionData.GravityMultiplier - 1.0f) : 1.0f) * Time.deltaTime;

            if (IsInAir)
            {
                // Jump / Fall enhancement
                if (m_DesiredVelocity.y < 0.0f)
                {
                    ContinuousJump = false;
                    m_DesiredVelocity += Vector3.up * Physics.gravity.y * (m_LocomotionData.FallMultiplier - 1) * Time.deltaTime;
                }
                else if (m_DesiredVelocity.y > 0.0f && !ContinuousJump)
                {
                    m_DesiredVelocity += Vector3.up * Physics.gravity.y * (m_LocomotionData.LowJumpMultiplier - 1) * Time.deltaTime;
                }
            }
            else
            {
                // Reset Vertical velocity
                m_DesiredVelocity.y = 0.0f;
            }

            // Update desired Velocity
            Vector3 newVelocity = m_CurrentMoveDirection * m_LocomotionData.RunSpeed;
            newVelocity.y = m_DesiredVelocity.y;
            m_DesiredVelocity = newVelocity;

            // Apply movement
            m_Controller.Move(m_DesiredVelocity * Time.deltaTime);

            IsInAir = !m_Controller.isGrounded;
        }
    }
}
