using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Eclipse.Weapons
{
    public class BaseWeapon : NetworkBehaviour
    {
        [SerializeField] protected Transform fireTransform;

        [SerializeField, Header("Ammo")] protected int maxAmmo;
        protected int currentAmmo;

        [SerializeField, Header("Firing"), Tooltip("The time, in seconds, between each shot")] protected float timeBetweenShots;
        [SerializeField, Tooltip("Can the trigger be held down to fire?")] protected bool canAutoFire;
        [SerializeField] protected bool fireReady;
        [SerializeField, Tooltip("How many times can the gun be fired before having to reset it?" +
            "\nWhen zero, this behaviour is disabled.")]
        protected int shotsBeforeRechamber;
        protected int currentShotsBeforeRechamber;
        [SerializeField, Tooltip("Is the weapon able to fire?")] protected bool chambered;
        [SerializeField, Tooltip("How many times you can shoot before having to release the trigger?\n" +
            "When zero, this behaviour is disabled.")] protected int burstFireCount;
        protected bool weaponEquipped;
        protected Rigidbody weaponRB;
        [SerializeField] protected bool fireInput;
        [SerializeField] protected bool firePressed;
        [SerializeField] protected GameObject bulletTrail;
        [SerializeField] protected ParticleSystem fireParticles;
        [SerializeField] protected float trailSpeed;
        [SerializeField] protected int bulletsPerShot;
        [SerializeField] protected float bulletRange;
        [SerializeField] protected LayerMask bulletLayermask;
        [SerializeField, Header("Weapon Spread"), Tooltip("The weapon's base spread, which applies even when aimed down the sights.")] protected Vector2 defaultSpread;
        [SerializeField, Tooltip("The weapon's additive spread, which only applies when hip-firing consistently")] protected Vector2 additiveSpread;
        [SerializeField, Tooltip("The weapon's current max additive spread")] protected float currentAddSpread;
        [SerializeField, Tooltip("How fast spread increases")] protected float spreadPerShot;
        [SerializeField, Tooltip("How fast spread decays")] protected float spreadDecay;
        [SerializeField, Tooltip("How much linear recoil to add per shot")] protected Vector3 linearRecoil;
        [SerializeField, Tooltip("How much angular recoil to add per shot")] protected Vector3 angularRecoil;
        public void PickupWeapon(WeaponManager newManager, bool isPlayer)
        {
            if (isPlayer)
            {
                GrabWeaponServerRPC(newManager.GetComponent<NetworkObject>().OwnerClientId);
            }
            weaponEquipped = true;
            CheckWeaponPhysics();
            fireTransform = newManager.GetFireTransform;
            NetworkObject.TrySetParent(newManager.transform,false);
        }
        public void DropWeapon()
        {
            DropWeaponServerRPC();
            weaponEquipped = false;
            CheckWeaponPhysics();
            NetworkObject.TrySetParent((Transform)null, true);
        }
        [ServerRpc]
        public void DropWeaponServerRPC()
        {
            NetworkObject.RemoveOwnership();
        }

        [ServerRpc]
        void GrabWeaponServerRPC(ulong clientID)
        {
            NetworkObject.ChangeOwnership(clientID);
        }
        void CheckWeaponPhysics()
        {
            weaponRB.isKinematic = weaponEquipped;
            foreach (var item in GetComponentsInChildren<Collider>(true))
            {
                item.enabled = !weaponEquipped;
            }
        }
        private void Start()
        {
            weaponRB = GetComponent<Rigidbody>();
            chambered = true;
            fireReady = true;
        }

        private void FixedUpdate()
        {
            CheckFireInput();
        }
        public void SetFireInput(bool input)
        {
            fireInput = input;
        }
        public virtual void CheckFireInput()
        {
            if (fireInput)
            {
                if(canAutoFire || (!firePressed && !canAutoFire))
                {
                    TryFire();
                    firePressed = true;
                }
            }
            else
            {
                firePressed = false;
            }
        }

        public virtual void TryFire()
        {
            if (chambered && fireReady)
            {
                if (burstFireCount > 0)
                {
                    StartCoroutine(BurstFire());
                }
                else
                {
                    PreFire(true);
                }
            }
        }
        public void PreFire(bool regularFire)
        {
            FireParticlesClientRPC();
            FireWeaponServerRPC(regularFire);
            SendRecoil();
        }
        [ClientRpc()]
        void FireParticlesClientRPC()
        {
            fireParticles.Play(true);
        }
        [ServerRpc]
        public virtual void FireWeaponServerRPC(bool regularFire)
        {
            int bullets = bulletsPerShot > 0 ? bulletsPerShot : 1;
            Vector3 fireDirection;
            Vector2 additiveSpreadLerp = Vector2.Lerp(Vector2.zero, additiveSpread, currentAddSpread);
            Vector2 spreadVector = defaultSpread + additiveSpreadLerp;
            RaycastHit hit = new();
            for (int i = 0; i < bullets; i++)
            {
                fireDirection = fireTransform.TransformDirection(new Vector3(Random.Range(-spreadVector.x, spreadVector.x), Random.Range(-spreadVector.y, spreadVector.y), bulletRange));
                if (Physics.Raycast(fireTransform.position, fireDirection, out hit, bulletRange, bulletLayermask, QueryTriggerInteraction.Ignore))
                {
                    DoBulletTrailClientRPC(hit.point, hit.distance);
                }
                else
                {
                    DoBulletTrailClientRPC(fireTransform.forward * bulletRange, bulletRange);
                }
                Debug.DrawRay(fireTransform.position, fireDirection, hit.collider ? Color.green : Color.red, 0.2f, false);
            }

            if (regularFire)
            {
                StartCoroutine(RegularFireWait());
            }

            if(shotsBeforeRechamber > 0)
            {
                currentShotsBeforeRechamber++;
                if(currentShotsBeforeRechamber >= shotsBeforeRechamber)
                {
                    chambered = false;
                }
            }
        }
        [ClientRpc()]
        public void DoBulletTrailClientRPC(Vector3 end, float distance)
        {
            GameObject trail = Instantiate(bulletTrail, fireTransform.position, Quaternion.identity);
            StartCoroutine(TrailLerp(trail.transform, trail.transform.position, end, distance / trailSpeed));
        }
        IEnumerator BurstFire()
        {
            fireReady = false;
            int currentBurstFireCount = 0;
            var wfs = new WaitForSeconds(timeBetweenShots);
            while (currentBurstFireCount < burstFireCount)
            {
                PreFire(false);
                currentBurstFireCount++;
                yield return wfs;
            }
            fireReady = true;
            yield return new WaitForFixedUpdate();
        }
        IEnumerator RegularFireWait()
        {
            fireReady = false;
            var wfs = new WaitForSeconds(timeBetweenShots);
            yield return wfs;
            fireReady = true;
            yield return new WaitForFixedUpdate();
        }
        public void RechamberWeapon()
        {
            chambered = true;
        }
        IEnumerator TrailLerp(Transform trail, Vector3 startPosition, Vector3 endPosition, float time)
        {
            float t = 0;
            var wff = new WaitForEndOfFrame();
            while (t < time)
            {
                float lerp = Mathf.InverseLerp(0, time, t);
                trail.position = Vector3.Lerp(startPosition, endPosition, lerp);
                t += Time.deltaTime;
                yield return wff;
            }
            Destroy(trail.gameObject, 1f);
        }
        void SendRecoil()
        {
            Vector3 ang = RandomRecoilVector(angularRecoil), lin = RandomRecoilVector(linearRecoil);
            GetComponentInParent<RigidbodyPlayerMotor>().ReceiveRecoil(ang, lin);
        }
        Vector3 RandomRecoilVector(Vector3 vec)
        {
            return new Vector3(Random.Range(0, vec.x),
                Random.Range(-vec.y, vec.y),
                Random.Range(-vec.z, vec.z));
        }
    }
}