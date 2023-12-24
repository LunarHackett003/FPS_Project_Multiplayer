using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Eclipse.Weapons
{
    public class BaseWeapon : NetworkBehaviour
    {
        RigidbodyPlayerMotor rpbm;
        [SerializeField] protected Transform fireTransform, trailTransform;

        [SerializeField, Header("Ammo")] protected int maxAmmo;
        protected int currentAmmo;
        public NetworkVariable<bool> currentlyOwned = new();
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
        [SerializeField, Tooltip("The delay after firing a burst")] protected float timeBetweenBursts;
        protected bool weaponEquipped;
        protected Rigidbody weaponRB;
        [SerializeField, Tooltip("The projectile fired. If null, the weapon will be hitscan.")] protected GameObject projectile;
        [SerializeField, Tooltip("The velocity of the projectile. If 0, the weapon will be hitscan.")] protected float projectileForwardVelocity;
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
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            weaponRB = GetComponent<Rigidbody>();
        }

        public void PickupWeapon(WeaponManager newManager, bool isPlayer)
        {
            if (isPlayer)
            {
                GrabWeaponServerRPC(newManager.OwnerClientId);
            }
            weaponEquipped = true;
            CheckWeaponPhysics();
            fireTransform = newManager.GetFireTransform;
            rpbm = newManager.GetComponent<RigidbodyPlayerMotor>();
        }
        public void DropWeapon()
        {
            DropWeaponServerRPC();
            weaponEquipped = false;
            CheckWeaponPhysics();
            rpbm = null;
        }
        [ServerRpc]
        public void DropWeaponServerRPC()
        {
            NetworkObject.RemoveOwnership();
            currentlyOwned.Value = false;
            NetworkObject.TrySetParent((Transform)null, true);
        }

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        void GrabWeaponServerRPC(ulong clientID)
        {
            Debug.Log($"Transferring weapon to {clientID}");
            NetworkObject.ChangeOwnership(clientID);
            currentlyOwned.Value = true;
            NetworkObject.TrySetParent(NetworkManager.ConnectedClients[clientID].PlayerObject.transform, false);
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
            currentAddSpread = Mathf.Clamp01(currentAddSpread - (spreadDecay * Time.fixedDeltaTime));
            if (weaponEquipped != currentlyOwned.Value)
            {
                weaponEquipped = currentlyOwned.Value;
                CheckWeaponPhysics();
            }
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
            if (projectile == null || projectileForwardVelocity <= 0)
            {
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
                        DoBulletTrailClientRPC(trailTransform.position + fireDirection, bulletRange);
                    }
                    Debug.DrawRay(fireTransform.position, fireDirection, hit.collider ? Color.green : Color.red, 0.2f, false);
                }
            }
            else
            {
                fireDirection = fireTransform.TransformDirection(new Vector3(Random.Range(-spreadVector.x, spreadVector.x), Random.Range(-spreadVector.y, spreadVector.y), projectileForwardVelocity));
                SpawnProjectileServerRPC(fireDirection);
            }
            if (regularFire)
            {
                StartCoroutine(RegularFireWait(timeBetweenShots));
            }

            if(shotsBeforeRechamber > 0)
            {
                currentShotsBeforeRechamber++;
                if(currentShotsBeforeRechamber >= shotsBeforeRechamber)
                {
                    chambered = false;
                }
            }
            currentAddSpread += spreadPerShot;
        }
        [ServerRpc]
        public void SpawnProjectileServerRPC(Vector3 direction)
        {
            GameObject proj = Instantiate(projectile, fireTransform.position + (fireTransform.forward * 0.2f), projectile.transform.rotation);
            proj.GetComponent<NetworkObject>().Spawn();
            proj.GetComponent<Rigidbody>().velocity = direction* projectileForwardVelocity;
        }
        [ClientRpc()]
        public void DoBulletTrailClientRPC(Vector3 end, float distance)
        {
            GameObject trail = Instantiate(bulletTrail, trailTransform.position, Quaternion.identity);
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
            StartCoroutine(RegularFireWait(timeBetweenBursts));
            yield return new WaitForFixedUpdate();
        }
        IEnumerator RegularFireWait(float time)
        {
            fireReady = false;
            var wfs = new WaitForSeconds(time);
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
            trail.position = endPosition;
            Destroy(trail.gameObject, 2f);
        }
        void SendRecoil()
        {
            Vector3 ang = RandomRecoilVector(angularRecoil), lin = LinearRecoilVector(linearRecoil);
            rpbm.ReceiveRecoil(ang, lin);
        }
        Vector3 LinearRecoilVector(Vector3 range)
        {
            return new Vector3(Random.Range(0, range.x),
                Random.Range(0, range.y),
                Random.Range(0, range.z));
        }
        Vector3 RandomRecoilVector(Vector3 range)
        {
            return new Vector3(Random.Range(0, range.x),
                Random.Range(-range.y, range.y),
                Random.Range(-range.z, range.z));
        }
    }
}