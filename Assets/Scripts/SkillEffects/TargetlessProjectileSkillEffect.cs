// Target Projectile skill effects like arrows, flaming fire balls, etc. that
// fly towards a direction without a target, and deal damage if anything hit.
//
// Note: we could move it on the server and use NetworkTransform to synchronize
// the position to all clients, which is the easy method. But we just move it on
// the server AND on the client to save bandwidth. Same result.
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class TargetlessProjectileSkillEffect : SkillEffect
{
    [Header("Components")]
    public Animator animator;

    [Header("Properties")]
    public float speed = 25;
    [HideInInspector] public int damage = 1; // set by skill
    [HideInInspector] public float stunChance; // set by skill
    [HideInInspector] public float stunTime; // set by skill
    [HideInInspector] public bool peircing;
    public bool basedPosition = true;
    public bool basedRotation = true;
    public TargetBuffSkill debuff;
    public TargetlessProjectileSkillEffect castOnDestroy;
    public List<FollowupJutsu> followupJutsu = new List<FollowupJutsu>();

    // client player.lookDirection might be different from server lookDirection
    // when firing. so we need to sync the true direction to the client to move
    // the projectile the same way as on the server.
    // prevents issues like https://github.com/vis2k/uMMORPG2D/issues/3

    // fly direction
    [SyncVar, HideInInspector] public Vector2 direction;

    // if a targetless projectile doesn't hit anything, it should still be
    // destroyed after a while so it doesn't hang around forever.
    // destroying after 'distance' instead of 'time' is more accurate.
    // => fireballs should only fly X meters far
    // => instead of say '10 seconds' of flying across the whole map
    public float autoDestroyDistance = 20;
    public float deathTimer = 30;

    Vector2 initialPosition;

    // effects like a trail or particles need to have their initial positions
    // corrected too. simply connect their .Clear() functions to the event.
    public UnityEvent onSetInitialPosition;
    public float startingDistance = 0;

    void Start()
    {
        // remember start position for distance checks
        initialPosition = transform.position;

        // move via Rigidbody into synced direction on server & client
        GetComponent<Rigidbody2D>().velocity = direction * speed;
    }

    public override void OnStartClient()
    {
        SetInitialPosition();
    }

    void SetInitialPosition()
    {
        // the projectile should always start at the effectMount position.
        // -> server doesn't run animations, so it will never spawn it exactly
        //    where the effectMount is on the client by the time the packet
        //    reaches the client.
        // -> the best solution is to correct it here once
        if (caster != null)
        {
            if(basedPosition) transform.position = caster.skills.effectMount.position + (Vector3)(caster.lookDirection * startingDistance);
            if(basedRotation)
            {
                float rot_z = Mathf.Atan2(caster.lookDirection.y, caster.lookDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, rot_z + 90);
            }
            onSetInitialPosition.Invoke();
        }
    }

    // fixedupdate on client and server to simulate the same effect without
    // using a NetworkTransform
    void FixedUpdate()
    {
        if (isServer)
        {
            deathTimer -= Time.deltaTime;
            for(int i = followupJutsu.Count - 1; i >= 0; i--)
            {
                followupJutsu[i].timeBeforeActive -= Time.deltaTime;
                if(followupJutsu[i].timeBeforeActive < 0)
                {
                    SpawnJutsu(followupJutsu[i].jutsuEffect);
                    followupJutsu.RemoveAt(i);
                }
            }

        }
        // caster still around?
        // note: we keep flying towards it even if it died already, because
        //       it looks weird if fireballs would be canceled inbetween.
        if (caster != null)
        {
            // server: did we fly further than auto destroy distance?
            if ((isServer && Vector2.Distance(initialPosition, transform.position) >= autoDestroyDistance) || (isServer && deathTimer < 0))
            {
                NetworkServer.Destroy(gameObject);
            }
        }
        else if (isServer) NetworkServer.Destroy(gameObject);
    }

    [ServerCallback]
    void OnTriggerEnter2D(Collider2D co)
    {
        Entity entity = co.GetComponentInParent<Entity>();
        // hit something that is not the caster?
        // we instantiate it near the caster, it's possible to hit self.
        // and something that the caster can actually attack?
        if (entity != null && entity != caster && caster.CanAttack(entity))
        {
            if (entity.health.current > 0)
            {
                caster.combat.DealDamageAt(entity, caster.combat.damage + damage, stunChance, stunTime);

                if(debuff)
                {
                    // add buff or replace if already in there
                    entity.skills.AddOrRefreshBuff(new Buff(debuff, 1));
                    GameObject go = Instantiate(debuff.effect.gameObject, entity.transform.position, Quaternion.identity);
                    BuffSkillEffect effectComponent = go.GetComponent<BuffSkillEffect>();
                    effectComponent.caster = caster;
                    effectComponent.target = entity;
                    effectComponent.buffName = name;
                    NetworkServer.Spawn(go);
                }
            }
            if(!peircing)
            {
                NetworkServer.Destroy(gameObject);
                return;
            }
        }

        if (co.tag == "ProjectileBlock" && !peircing)
        {
            destroy();
            return;
        }
    }

    private void destroy()
    {
        NetworkServer.Destroy(gameObject);
    }

    [ServerCallback]
    private void OnDisable()
    {
        if (castOnDestroy) SpawnJutsu(castOnDestroy);
    }

    [Server]
    void SpawnJutsu(TargetlessProjectileSkillEffect wantedJutsu)
    {
        GameObject go = Instantiate(wantedJutsu.gameObject, transform.position, Quaternion.identity);

        TargetlessProjectileSkillEffect effect = go.GetComponent<TargetlessProjectileSkillEffect>();
        effect.target = caster.target;
        effect.caster = caster;
        effect.damage = damage;
        effect.stunChance = stunChance;
        effect.stunTime = stunTime;
        effect.peircing = peircing;
        // always fly into caster's look direction.
        // IMPORTANT: use the parameter. DON'T use entity.direction.
        // we want the exact direction that was passed in CmdUse()!
        effect.direction = caster.lookDirection;//direction;
        NetworkServer.Spawn(go);
    }

    // animation (if any)
    [ClientCallback]
    void Update()
    {
        if (animator != null)
        {
            animator.SetFloat("DirectionX", direction.x);
            animator.SetFloat("DirectionY", direction.y);
        }
    }
}

[System.Serializable]
public class FollowupJutsu
{
    public TargetlessProjectileSkillEffect jutsuEffect;
    public float timeBeforeActive;
}