using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RigidbodyPlayerMotor : ManagedBehaviour
{
    public enum MovementState
    {
        none = 0,
        onFoot = 1,
        sliding = 2,
        airborne = 3,
    }
    //----------------------------------------------
    //Aim
    //----------------------------------------------
    [Header("Aim")]
    [SerializeField, Tooltip("Should aiming horizontally use the Head-Y transform")] bool useHeadY;
    [SerializeField, Tooltip("Aiming transforms")] Transform headX, headY;
    [SerializeField] Vector2 lookAngle, finalLookAngle, lookSpeed;
    [SerializeField, Tooltip("Slide-related setting.")] float slideCameraAngle, slideTiltTime, slideCurrentTilt;
    float slideTiltLerpVelocity;
    //----------------------------------------------
    //Movement
    //----------------------------------------------
    [Header("Movement"), SerializeField, Tooltip("Regular movement speed")] Vector2 runForce;
    [SerializeField, Tooltip("Movement speed in other state")] Vector2 sprintForce, crouchWalkForce;
    [SerializeField, Tooltip("Velocity/Force setting")] float slideVelocity, airDodgeVelocity, doubleJumpVelocity, boostSlideForce, airborneControlMultiplier;
    [SerializeField, Tooltip("Drag applied in movement states")] float walkDrag, slideDrag, airborneDrag;
    [SerializeField, Tooltip("Is the player sprinting?")] bool sprinting;
    [SerializeField, Tooltip("The speed under which the player stops, or cannot start sliding")] float slideVelocityThreshold;
    [SerializeField, Tooltip("How far to push the view down when jumping")] float jumpScreenBounce;
    [SerializeField, Tooltip("Is double-jumping enabled?")] bool doubleJumpEnabled;
    public void SetDoubleJumpActive(bool activated)
    {
        doubleJumpEnabled = activated;
    }
    bool doubleJumped;
    bool canDoubleJump;
    [SerializeField] MovementState moveState;
    //----------------------------------------------
    //Wall-riding
    //----------------------------------------------
    public void SetWallrunActive(bool activated)
    {
        wallRideEnabled = activated;
    }
    [Header("Wall-Riding"), SerializeField, Tooltip("Is wallriding enabled?")] bool wallRideEnabled;
    [SerializeField, Tooltip("Is the player wallriding?")] bool wallriding;
    [SerializeField, Tooltip("Wallride force")] float wallrideMoveForce, wallrideStickForce;
    [SerializeField, Tooltip("The normal of the surface the player is wallriding")] Vector3 wallrideNormal;
    [SerializeField, Tooltip("Can the player wallride on similar normals twice?")] bool allowRecling;
    [SerializeField, Tooltip("Is the player waiting for a recling?")] bool awaitingRecling;
    [SerializeField, Tooltip("How long it takes to clear the wallride normal between wallrides")] float wallrideNormalClearTime;
    [SerializeField, Tooltip("How similar can the new wall normal and current wall normal be to prevent reclinging?")] float wallReclingSimilarityThreshold;
    float currentWallrideNormalClearTime;
    [SerializeField, Tooltip("Wallride cancel setting")] float wallrideMaxTime, wallrideCurrentTime, wallrideVelocityThreshold;
    [SerializeField, Tooltip("The gravity counter force over time, from 0-1")] AnimationCurve wallrideGravityInfluence;
    [SerializeField, Tooltip("The force applied against gravity while wallriding")] float wallrideAntiGravityForce;
    [SerializeField, Tooltip("Wallride Capsule Cast Setting")] float wallrideCheckDistance, wallrideCapsuleCastRadius, wallrideCapsuleCastHeight;
    [SerializeField, Tooltip("Wallride Capsule Cast Setting")] float wallrideCheckOriginHeight;
    [SerializeField, Tooltip("Wallride Capsule Cast Setting")] float wallrideEnableTimeAfterGrounded, wallrideCurrentTimeAfterGrounded;
    [SerializeField, Tooltip("How quickly to lerp towards the wall-run normal, to help keep aligned with the wallrun")] float wallrunNormalLerpTime;
    bool wallrideEnabledAfterGrounded;
    bool wallOnRight;
    //----------------------------------------------
    //Ground Check
    //----------------------------------------------
    [Header("Physics")]
    [SerializeField, Tooltip("The player's rigidbody")] Rigidbody rb;
    [SerializeField, Tooltip("The minimum force required to apply movement forces/velocity on a downward impact.")] float collisionImpulseSlideTrigger = 30;
    //----------------------------------------------
    //Ground Check
    //----------------------------------------------
    [Header("Ground Check")]
    [SerializeField, Tooltip("The normal of the ground currently walked on")] Vector3 groundNormal;
    [SerializeField, Tooltip("The offset position for ground checking")] Vector3 groundCheckOffset;
    [SerializeField, Tooltip("Ground Check Setting")] float groundCheckDistance, groundCheckRadius;
    [SerializeField, Tooltip("The layermask for the ground check spherecast")] LayerMask groundCheckLayermask;
    [SerializeField, Tooltip("The minimum dot product threshold of the player's up direction and the ground normal")] float walkableGroundThreshold;
    bool crouching;
    //---------------------------------------------
    //Headbobbing
    //---------------------------------------------
    [Header("Head-bobbing"), SerializeField, Tooltip("The transform that is moved for headbobbing")] Transform headbobTransform;
    [SerializeField, Tooltip("The multiplier to the headbob amount")] float headbobMultiplier;
    [SerializeField, Tooltip("The headbob position")] Vector3 headbobPosition;
    [SerializeField, Tooltip("The extents of the headbob")] Vector2 bobExtents;
    [SerializeField, Tooltip("Axis Bob animation curves")] AnimationCurve xBobCurve, yBobCurve;
    [SerializeField, Tooltip("Headbob movement influence - x axis is when idling, y axis is when moving at full speed")] Vector2 headbobSizeInfluence;
    [SerializeField, Tooltip("Head bob movement speed influence - x axis when idling, y axis when moving at full speed")] Vector2 headbobSpeedInfluence;
    [SerializeField, Tooltip("Head bob sprint multiplier - x is sizeMultiplier, y is speed multiplier")] Vector2 headbobSprintMultiplier;
    [SerializeField, Tooltip("Head bob speed multiplier")] Vector2 headbobSpeedMultiplier;
    [SerializeField, Tooltip("Should the x and y axes of the bob lerp vector be swapped?")] bool swizzleBobLerp;
    [SerializeField, Tooltip("How fast the camera should lerp towards the target vector")] float headbobLerpSpeed;
    [SerializeField]
    float crouchLerpVelocity;
    [SerializeField] bool useFixedUpdate;
    Vector2 lookInput;
    Vector2 moveInput;
    [SerializeField] bool sliding;
    [SerializeField] bool sprintInput;
    [SerializeField] PlayerCapsuleSizeControl pcsc;
    [SerializeField, Header("HUD Wobble"), Tooltip("HUD Wobble Transform")] RectTransform HUDWobbleTransform;
    [SerializeField, Tooltip("HUD Wobble Multiplier")] float HUDWobbleMultiplier;

    [SerializeField] Vector3 currentRecoilAngle, currentRecoilPosition, targetRecoilAngle, targetRecoilPosition;
    [SerializeField] float angularRecoilDecay, linearRecoilDecay, angularRecoilMultiplier, linearRecoilMultiplier, linearRecoilLerpSpeed, angularRecoilLerpSpeed, recoilAimMultipler;
    public void ReceiveRecoil(Vector3 angularRecoil, Vector3 linearRecoil)
    {
        targetRecoilAngle += angularRecoil;
        targetRecoilPosition += linearRecoil;
    }
    /// <summary>
    /// Sets the look input on this motor
    /// </summary>
    /// <param name="inputObject">The object sending the input. Will not apply input if it is not owned by this player!</param>
    /// <param name="input">The input vector</param>
    public void SetLookInput(MonoBehaviour inputObject, Vector2 input)
    {
        if (inputObject.transform.root == transform.root)
        {
            lookInput = input;
        }
        else
        {
            Debug.Log("Incorrect Object is sending look input!");
        }
    }
    /// <summary>
    /// Sets the move input on this motor
    /// </summary>
    /// <param name="inputObject">The object sending the input. Will not apply input if it is not owned by this player!</param>
    /// <param name="input">The input vector</param>
    public void SetMoveInput(MonoBehaviour inputObject, Vector2 input)
    {
        if(inputObject.transform.root == transform.root)
        {
            moveInput = input;
        }
        else
        {
            Debug.Log("Incorrect Object is sending look input!");
        }
    }
    public bool GetCrouching()
    {
        return crouching;
    }
    private void Start()
    {

    }
    public override void ManagedPreUpdate()
    {
        base.ManagedPreUpdate();
        if (!useFixedUpdate)
            LookCalcs();
    }
    public override void ManagedFixedUpdate()
    {
        pcsc.SetCrouch(crouching);
        if(useFixedUpdate)
            LookCalcs();
        GroundCheck();
        if (moveState == MovementState.onFoot && wallriding)
            CancelWallride(false);
        if (moveState == MovementState.sliding && rb.GetLateralVelocity().magnitude < slideVelocityThreshold * 0.5f)
        {
            sliding = false;
        }
        rb.drag = moveState switch { MovementState.onFoot => walkDrag, MovementState.sliding => slideDrag, MovementState.airborne => airborneDrag, _ => 0};
        
        if (!wallriding)
            MoveCalcs();
        WallrideUpdate();
        HeadbobUpdate();
    }
    void HeadbobUpdate()
    {
        
        //  * Mathf.InverseLerp(headbobSizeInfluence.x, headbobSizeInfluence.y, moveSqrMag)
        float moveSqrMag = moveInput.sqrMagnitude;
        float timeMult = Time.time * (sprinting ? headbobSprintMultiplier.x : 1) * Mathf.Lerp(headbobSpeedMultiplier.x, headbobSpeedMultiplier.y, moveSqrMag);
        float xLerp = (Mathf.Sin(timeMult * headbobSpeedMultiplier.x) + 1) / 2;
        float yLerp = (Mathf.Cos(timeMult * headbobSpeedMultiplier.y) + 1) / 2;
        Vector2 bobExtentMult = (sprinting ? headbobSprintMultiplier.y : 1 ) * bobExtents * Mathf.Lerp(headbobSizeInfluence.x, headbobSizeInfluence.y, moveSqrMag);
        Vector2 bobLerp = new(swizzleBobLerp ? xLerp : yLerp , swizzleBobLerp ? yLerp : xLerp);
        Vector2 bobPos = new(xBobCurve.Evaluate(bobLerp.x) * bobExtentMult.x, yBobCurve.Evaluate(bobLerp.y) * bobExtentMult.y);
        headbobPosition = bobPos * headbobMultiplier;
        headbobTransform.localPosition = Vector3.Lerp(headbobTransform.localPosition, headbobPosition, Time.fixedDeltaTime * headbobLerpSpeed);
        headbobTransform.localRotation = Quaternion.identity;
        if (HUDWobbleTransform)
        {
            HUDWobbleTransform.localPosition = headbobTransform.localPosition * HUDWobbleMultiplier;
        }

        currentRecoilPosition = Vector3.Lerp(currentRecoilPosition, targetRecoilPosition, linearRecoilLerpSpeed * Time.fixedDeltaTime);
        currentRecoilAngle = Vector3.Lerp(currentRecoilAngle, targetRecoilAngle, linearRecoilLerpSpeed * Time.fixedDeltaTime);
        targetRecoilPosition = Vector3.Lerp(targetRecoilPosition, Vector3.zero, Time.fixedDeltaTime * linearRecoilDecay);
        targetRecoilAngle = Vector3.Lerp(targetRecoilAngle, Vector3.zero, Time.fixedDeltaTime * angularRecoilDecay);
        headbobTransform.localPosition += currentRecoilPosition * linearRecoilMultiplier;
        headbobTransform.localRotation *= Quaternion.Euler(currentRecoilAngle * angularRecoilMultiplier);
        finalLookAngle = lookAngle + (new Vector2 (currentRecoilAngle.y, currentRecoilAngle.x) * recoilAimMultipler);
    }

    /// <summary>
    /// Performs the wallride physics queries, applies forces, etc
    /// </summary>
    void WallrideUpdate()
    {
        if (wallriding)
        {
            WallrideCast();
            Vector3 wallrideVector = Vector3.Project(transform.forward, Vector3.Cross(transform.up, wallrideNormal));
            rb.AddForce(-wallrideNormal * wallrideStickForce + (wallrideMoveForce * moveInput.y * wallrideVector + (wallrideGravityInfluence.Evaluate(Mathf.InverseLerp(0, wallrideMaxTime, wallrideCurrentTime)) * rb.mass * -Physics.gravity)));
            wallrideCurrentTime += Time.fixedDeltaTime;
            transform.right = Vector3.Lerp(transform.right, wallOnRight ? -wallrideNormal : wallrideNormal, Time.fixedDeltaTime * wallrunNormalLerpTime);

            if(rb.GetLateralVelocity().magnitude < wallrideVelocityThreshold)
            {
                CancelWallride(false);
            }
        }
        else
        {
            wallrideCurrentTime = 0;
            if (moveState == MovementState.onFoot)
            {
                wallrideEnabledAfterGrounded = false;
                wallrideCurrentTimeAfterGrounded = 0;
            }
            else
            {
                wallrideCurrentTimeAfterGrounded += Time.fixedDeltaTime;
                wallrideEnabledAfterGrounded = wallrideCurrentTimeAfterGrounded > wallrideEnableTimeAfterGrounded;
                
            }

            if(wallRideEnabled && wallrideEnabledAfterGrounded)
            {
                WallrideCast();
            }
        }


        if (awaitingRecling)
        {
            currentWallrideNormalClearTime += Time.fixedDeltaTime;
            if(currentWallrideNormalClearTime >= wallrideNormalClearTime)
            {
                awaitingRecling = false;
                wallrideNormal = Vector3.zero;
            }
        }
    }

    void WallrideCast()
    {
        Vector3 capsuleOriginPosition = transform.position + new Vector3(0, wallrideCheckOriginHeight, 0);
        Vector3 capsuleSize = Vector3.up * wallrideCapsuleCastHeight;
        bool rightWallNotHit = false;
        RaycastHit hit1, hit2 = new();
        if (!Physics.CapsuleCast(capsuleOriginPosition + capsuleSize, capsuleOriginPosition - capsuleSize, wallrideCapsuleCastRadius, transform.right, out hit1, wallrideCheckDistance, groundCheckLayermask))
        {
            rightWallNotHit = true;
        }
        if (rightWallNotHit)
        if(!Physics.CapsuleCast(capsuleOriginPosition + capsuleSize, capsuleOriginPosition - capsuleSize, wallrideCapsuleCastRadius, -transform.right, out hit2, wallrideCheckDistance, groundCheckLayermask))
            {
                if(wallriding)
                    CancelWallride(false);
                return;
            }
        RaycastHit hit = hit1.collider != null ? hit1 : hit2;
        wallOnRight = hit1.collider != null;
        if(!allowRecling && awaitingRecling && Vector3.Dot(hit.normal, wallrideNormal) > wallReclingSimilarityThreshold)
        {
            //Wallride normals are too similar and the player is not allowed to recling.
            if(wallriding)
                CancelWallride(false);
            return;
        }
        wallrideNormal = hit.normal;
        if (rb.GetLateralVelocity().magnitude < wallrideVelocityThreshold)
        {
            CancelWallride(false);
            return;
        }
        if (!wallriding)
        {
            rb.velocity = rb.GetLateralVelocity() + (Vector3.up * ( rb.velocity.y * 0.4f));
            lookAngle.x = 0;

        }
        wallriding = true;
        doubleJumped = false;
    }
    /// <summary>
    /// Drops and/or ejects the player from the wall
    /// </summary>
    /// <param name="eject"></param>
    void CancelWallride(bool eject)
    {
        wallriding = false;
        awaitingRecling = true;
        currentWallrideNormalClearTime = 0;
        transform.rotation = Quaternion.Euler(0, headY.eulerAngles.y, 0);
        headY.localRotation = Quaternion.identity;
        if (eject)
        {
            rb.velocity += ((wallrideNormal + Vector3.up).normalized * doubleJumpVelocity);
        }
    }

    /// <summary>
    /// Performs aiming calculations and rotates transforms
    /// </summary>
    void LookCalcs()
    {
        Vector2 lookInput_loc = lookInput;
        //lookInput_loc.SwizzleVector2();
        lookInput_loc.Scale(lookSpeed * (useFixedUpdate?  Time.fixedDeltaTime:Time.smoothDeltaTime));
        lookInput_loc.y *= -1;
        lookAngle += lookInput_loc;
        lookAngle.y.Clamp(-90, 90);
        lookAngle.x.Clamp(-45, 45);
        slideCurrentTilt = Mathf.SmoothDamp(slideCurrentTilt, moveState == MovementState.sliding ? slideCameraAngle : 0, ref slideTiltLerpVelocity, slideTiltTime);
        headX.localRotation = Quaternion.Euler(finalLookAngle.y, 0, slideCurrentTilt);
        if (wallriding)
        {
            float lookYaw = finalLookAngle.x * 0.9f;
            headY.localRotation = Quaternion.Euler(0, lookYaw, 0);
            transform.localRotation *= Quaternion.Euler(0, lookInput_loc.x * 0.1f, 0);
        }
        else
        {
            headY.localRotation = Quaternion.identity;
            transform.localRotation *= Quaternion.Euler(0, lookInput_loc.x, 0);
        }
        if (lookAngle.x > 360)
        {
            lookAngle.x -= 360;
        }
        if(lookAngle.x < -360)
        {
            lookAngle.x += 360;
        }
    }
    /// <summary>
    /// Performs movement calculations and applies forces
    /// </summary>
    void MoveCalcs()
    {
        if (moveState == MovementState.none)
            return;
        //Player cannot move, and move calculations will not be done.
        Vector2 moveForce = sprintInput ? sprintForce : ( crouching ? crouchWalkForce : runForce);
        Vector3 movement = (transform.right * moveForce.x * moveInput.x) + (transform.forward * moveForce.y * moveInput.y);
        Vector3 movementProjected = Vector3.ProjectOnPlane(movement, groundNormal);
        if (moveState == MovementState.onFoot)
        {
            rb.AddForce(movementProjected);
        }
        else
        {
            rb.AddForce(movement * airborneControlMultiplier);
        }
    }
    IEnumerator ResetDoubleJump()
    {
        canDoubleJump = false;
        yield return new WaitForSeconds(0.1f);
        canDoubleJump = true;
        yield return new WaitForFixedUpdate();
    }
    /// <summary>
    /// Spherecasts to detect the ground
    /// </summary>
    void GroundCheck()
    {
        if(Physics.SphereCast(transform.position + groundCheckOffset, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundCheckLayermask))
        {
            groundNormal = hit.normal;
            if (Vector3.Dot(transform.up, hit.normal) > walkableGroundThreshold)
            {
                if (!sliding)
                    moveState = MovementState.onFoot;
                else
                    moveState = MovementState.sliding;
                doubleJumped = false;
            }
        }
        else
        {
            moveState = MovementState.airborne;
        }
    }
    public bool GetSlideStatus()
    {
        return sliding;
    }
    public void PressCrouch(bool input)
    {
        sliding = input;
        crouching = input;
        Debug.Log($"player crouching - Sliding {sliding}");
        if (rb.GetLateralVelocity().magnitude > slideVelocityThreshold && moveState == MovementState.onFoot && sliding)
        {
            rb.velocity = rb.GetLateralVelocity().normalized * slideVelocity;
            moveState = MovementState.sliding;
        }
        else
        {
            CancelWallride(false);
        }
        if (crouching)
        {
            sprinting = false;
            sprintInput = false;
        }
    }
    public void Sprint()
    {
        Sprint(!sprintInput);
    }

        public void Sprint(bool input)
    {
        Debug.Log("player sprinting;");
        sprintInput = input;
        if (sprintInput)
        {
            crouching = false;
        }
    }
    public void SlideBoost()
    {
        Debug.Log("player slide boosting");
    }
    public void Jump()
    {
        if (!wallriding)
        {
            Debug.Log("Player jumping");
            if (moveState == MovementState.onFoot || moveState == MovementState.sliding)
            {
                JumpForce();
                StartCoroutine(ResetDoubleJump());
                doubleJumped = false;
            }
            else
            {
                if (!doubleJumped && canDoubleJump && doubleJumpEnabled)
                {
                    JumpForce();
                    
                    doubleJumped = true;
                }
            }
        }
        else
        {
            CancelWallride(true);
        }
    }
    void JumpForce()
    {
        rb.velocity = rb.GetLateralVelocity() + (doubleJumpVelocity * Vector3.up);
        headbobTransform.localPosition += Vector3.down * jumpScreenBounce;

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (Vector3.Dot(collision.GetContact(0).normal, Vector3.up) > walkableGroundThreshold && collision.impulse.y > collisionImpulseSlideTrigger)
        {
            float moveVel = sliding ? slideVelocity : 2;
            rb.velocity += transform.rotation * new Vector3(moveInput.x * moveVel, 0, moveInput.y * moveVel)  ;
        }
        if (wallriding && collision.impulse.magnitude > collisionImpulseSlideTrigger )
            rb.velocity += transform.forward * (wallrideMoveForce / rb.mass);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 topSphere = new(0, wallrideCapsuleCastHeight + wallrideCheckOriginHeight);
        Vector3 bottomSphere = new(0, +wallrideCheckOriginHeight - wallrideCapsuleCastHeight);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(topSphere, wallrideCapsuleCastRadius);
        Gizmos.DrawWireSphere(bottomSphere, wallrideCapsuleCastRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((Vector3.right * wallrideCheckDistance) + topSphere, wallrideCapsuleCastRadius);
        Gizmos.DrawWireSphere((Vector3.right * wallrideCheckDistance) + bottomSphere, wallrideCapsuleCastRadius);
        Gizmos.DrawWireSphere((Vector3.left * wallrideCheckDistance) + topSphere, wallrideCapsuleCastRadius);
        Gizmos.DrawWireSphere((Vector3.left * wallrideCheckDistance) + bottomSphere, wallrideCapsuleCastRadius);


    }
    void DrawHandleSphere(Vector3 position, float radius, Matrix4x4 matrix)
    {
        Handles.matrix = matrix;
        Handles.DrawWireArc(position, Vector3.up, Vector3.right, 360, radius);
        Handles.DrawWireArc(position, Vector3.forward, Vector3.right, 360, radius);
        Handles.DrawWireArc(position, Vector3.right, Vector3.up, 360, radius);
    }

#endif
}
