using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Eclipse.Weapons
{
    public class WeaponManager : NetworkBehaviour
    {
        [SerializeField] protected bool isPlayer;
        [SerializeField] protected Transform fireRoot;
        [SerializeField] protected int maxWeapons = 3;
        [SerializeField] protected int currentWeaponIndex = 0;
        [SerializeField] protected List<BaseWeapon> weapons;
        [SerializeField] protected PlayerInputCollector pic;
        [SerializeField] protected float weaponInteractDistance;
        [SerializeField] protected TextMeshProUGUI weaponNameText;
        [SerializeField] protected LayerMask weaponLayermask;
        [SerializeField] BaseWeapon currentTargetedWeapon;
        [SerializeField] Transform activeWeaponTransform;
        [SerializeField] Transform[] holsters;
        public void EquipWeapon(BaseWeapon bw)
        {
            Vector3 newWeaponPos = bw.transform.position;
            Quaternion newWeaponRot = bw.transform.rotation;
            if (weapons.Count > 0)
            {
                BaseWeapon oldweapon = weapons[currentWeaponIndex];
                if (weapons.Count >= maxWeapons)
                {
                    oldweapon.DropWeapon();
                    oldweapon.transform.SetPositionAndRotation(newWeaponPos, newWeaponRot);
                    weapons[currentWeaponIndex] = bw;
                }
                else
                {
                    weapons.Add(bw);
                }
            }
            else
            {
                weapons.Add(bw);
            }
            bw.PickupWeapon(this, isPlayer);
        }
        public Transform GetFireTransform { get { return fireRoot; } }
        private void FixedUpdate()
        {
            WeaponInteractionCheck();
        }
        private void LateUpdate()
        {
            if (weapons.Count > 0)
            {
                for (int i = 0; i < weapons.Count; i++)
                {
                    if (i == currentWeaponIndex)
                    {
                        weapons[i].transform.SetPositionAndRotation(activeWeaponTransform.position, activeWeaponTransform.rotation);
                    }
                    else
                    {
                        weapons[i].transform.SetPositionAndRotation(holsters[i].position, holsters[i].rotation);
                    }
                }
            }
        }

        public void WeaponInteractionCheck()
        {
            Debug.DrawRay(fireRoot.position + (transform.right * 0.1f), fireRoot.forward * weaponInteractDistance, Color.blue);
            if (Physics.Raycast(fireRoot.position, fireRoot.forward, out RaycastHit hit, weaponInteractDistance, weaponLayermask))
            {
                if (!hit.rigidbody)
                {
                    weaponNameText.text = "";
                    return;
                }
                hit.rigidbody.TryGetComponent(out BaseWeapon wp);
                if (wp)
                {
                    weaponNameText.text = wp.name;
                    currentTargetedWeapon = wp;
                }
                else
                {
                    weaponNameText.text = "";
                    currentTargetedWeapon = null;
                }
            }
            else
            {
                weaponNameText.text = "";
                currentTargetedWeapon = null;
            }
        }
        public void TryPickupWeapon(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            if (currentTargetedWeapon)
            {
                EquipWeapon(currentTargetedWeapon);
            }
        }
        public void FireInput(InputAction.CallbackContext context)
        {
            if (weapons.Count > 0)
            {
                weapons[currentWeaponIndex].SetFireInput(context.performed);
            }
        }
        public void SwitchWeaponNumber(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                currentWeaponIndex = Mathf.Clamp((int)context.ReadValue<float>() - 1, 0, weapons.Count-1) ;
            }
        }
        public void SwitchWeaponTap(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                currentWeaponIndex = Mathf.Clamp(currentWeaponIndex == 0 ? 1 : 0, 0, weapons.Count-1);
            }
        }
        public void SwitchWeaponHeavy(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                currentWeaponIndex = Mathf.Clamp(currentWeaponIndex == 2 ? 1 : 2, 0, weapons.Count - 1);
            }
        }
    }
}