using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCapsuleSizeControl : ManagedBehaviour
{
    public override void ManagedFixedUpdate()
    {
        base.ManagedFixedUpdate();
        CapsuleUpdate();
    }
    private void Start()
    {
        capsule.material = physicalMaterial;
    }

    //----------------------------------------------
    //Crouching
    //----------------------------------------------
    [Header("Crouching"), SerializeField, Tooltip("How tall the player is when crouching")] float crouchHeight;
    [SerializeField, Tooltip("How tall the player is when standing")] float standHeight;
    [SerializeField, Tooltip("Which transform to translate when crouching.")] Transform crouchTransform;
    [SerializeField, Tooltip("The current lerp progress between crouching and standing")] float crouchLerpAmount;
    [SerializeField, Tooltip("How long it takes to crouch")] float crouchLerpTime;
    [SerializeField, Tooltip("Is the player crouching?")] bool crouching;
    [SerializeField, Tooltip("The capsule height's offset from the head height")] float standingCapsuleHeightHeadBuffer, crouchingCapsuleHeightHeadBuffer;
    [SerializeField, Tooltip("The player's capsule collider")] CapsuleCollider capsule;

    [SerializeField, Tooltip("The player's physical material")] PhysicMaterial physicalMaterial;

    float crouchLerpVelocity;
    void CapsuleUpdate()
    {
        crouchLerpAmount = Mathf.SmoothDamp(crouchLerpAmount, crouching ? 0 : 1, ref crouchLerpVelocity, crouchLerpTime);

        crouchTransform.localPosition = Vector3.up * Mathf.Lerp(crouchHeight, standHeight, crouchLerpAmount);
        capsule.height = crouchTransform.localPosition.y + Mathf.Lerp(crouchingCapsuleHeightHeadBuffer, standingCapsuleHeightHeadBuffer, crouchLerpAmount);
        capsule.center = Vector3.up * (capsule.height / 2);
    }
}
