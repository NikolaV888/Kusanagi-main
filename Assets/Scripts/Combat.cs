using System;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public enum DamageType { Normal, Block, Crit }

// inventory, attributes etc. can influence max health
public interface ICombatBonus
{
    int GetDamageBonus();
    int GetDefenseBonus();
    float GetCriticalChanceBonus();
    float GetBlockChanceBonus();
}

[Serializable] public class UnityEventIntDamageType : UnityEvent<int, DamageType> {}

[DisallowMultipleComponent]
public class Combat : NetworkBehaviour
{
    [Header("Components")]
    public Level level;
    public Entity entity;
    public new Collider2D collider;

    [Header("Stats")]
    [SyncVar] public bool invincible = false; // GMs, Npcs, ...
    public LinearInt baseDamage = new LinearInt{baseValue=1};
    public LinearInt baseDefense = new LinearInt{baseValue=1};
    public LinearFloat baseBlockChance;
    public LinearFloat baseCriticalChance;

    [Header("Damage Popup")]
    public GameObject damagePopupPrefab;

    [SyncVar] public int hitCount = 0; 
    public int animationIndex = 0;

    // events
    [Header("Events")]
    public UnityEventEntity onDamageDealtTo;
    public UnityEventEntity onKilledEnemy;
    public UnityEventEntityInt onServerReceivedDamage;
    public UnityEventIntDamageType onClientReceivedDamage;

    // cache components that give a bonus (attributes, inventory, etc.)
    ICombatBonus[] _bonusComponents;
    ICombatBonus[] bonusComponents =>
        _bonusComponents ?? (_bonusComponents = GetComponents<ICombatBonus>());

    public AudioClip OnHitSFX;
    public AudioClip OnMissSFX;

    // calculate damage
    public int damage
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetDamageBonus();
            return baseDamage.Get(level.current) + bonus;
        }
    }

    // calculate defense
    public int defense
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetDefenseBonus();
            return baseDefense.Get(level.current) + bonus;
        }
    }

    // calculate block
    public float blockChance
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float bonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetBlockChanceBonus();
            return baseBlockChance.Get(level.current) + bonus;
        }
    }

    // calculate critical
    public float criticalChance
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            float bonus = 0;
            foreach (ICombatBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetCriticalChanceBonus();
            return baseCriticalChance.Get(level.current) + bonus;
        }
    }

    // combat //////////////////////////////////////////////////////////////////
    // deal damage at another entity
    // (can be overwritten for players etc. that need custom functionality)
    [Server]
    public virtual void DealDamageAt(Entity victim, int amount, float stunChance=0, float stunTime=0, float pushback=0, float pushforward = 0)
    {
        entity.target = victim;
        Debug.Log("Buffs active: " + victim.skills.buffs.Count);
        foreach (Buff buff in victim.skills.buffs)
        {
            if(buff.substitutePoint != Vector2.zero)
            {
                if (buff.substituteEffect != null)
                {
                    GameObject go = Instantiate(buff.substituteEffect.gameObject, victim.transform.position, Quaternion.identity);
                    SubstituteSkillEffect effectComponent = go.GetComponent<SubstituteSkillEffect>();
                    NetworkServer.Spawn(go);
                }

                victim.movement.Warp(buff.substitutePoint);
                victim.skills.buffs.Remove(buff);
                return;
            }
        }

        Combat victimCombat = victim.combat;
        int damageDealt = 0;
        DamageType damageType = DamageType.Normal;

        // don't deal any damage if entity is invincible
        if (!victimCombat.invincible)
        {
            //old block system
            if (false && victim.state == Entity.States.Blocking.ToString())
            {
                damageType = DamageType.Block;
            }
            // deal damage
            else
            {
                // subtract defense (but leave at least 1 damage, otherwise
                // it may be frustrating for weaker players)
                damageDealt = Mathf.Max(amount - victimCombat.defense, 1);
                if (victim.state == Entity.States.Blocking.ToString()) damageDealt /= 2;
                // critical hit?
                if (UnityEngine.Random.value < criticalChance)
                {
                    damageDealt *= (int)1.1f;
                    damageType = DamageType.Crit;
                }

                // deal the damage
                victim.health.current -= damageDealt;

                //pushback
                if (pushback > 0.1f)
                {
                    victim.movement.Warp((Vector2)victim.transform.position + (entity.lookDirection * pushback));
                }

                //pushforward ie enemy movement
                if (pushforward > 0.1f)
                {
                    entity.movement.Warp((Vector2)entity.transform.position + (entity.lookDirection * pushforward));
                    //victim.stunTimeEnd = 0;
                }

                // call OnServerReceivedDamage event on the target
                // -> can be used for monsters to pull aggro
                // -> can be used by equipment to decrease durability etc.
                victimCombat.onServerReceivedDamage.Invoke(entity, damageDealt);

                // stun?
                if (UnityEngine.Random.value < stunChance)
                {
                    // dont allow a short stun to overwrite a long stun
                    // => if a player is hit with a 10s stun, immediately
                    //    followed by a 1s stun, we don't want it to end in 1s!
                    double newStunEndTime = NetworkTime.time + stunTime;
                    //victim.stunTimeEnd = Math.Max(newStunEndTime, entity.stunTimeEnd); 
                    if (stunTime > 0) victim.stunTimeEnd = newStunEndTime;
                    else victim.stunTimeEnd = 0;
                    Debug.Log("Stun time: " + stunTime);
                    Debug.Log("newStunEndTime: " + newStunEndTime);
                    Debug.Log("victim.stunTimeEnd: " + victim.stunTimeEnd);
                }
            }

            // call OnDamageDealtTo / OnKilledEnemy events
            onDamageDealtTo.Invoke(victim);
            if (victim.health.current == 0)
                onKilledEnemy.Invoke(victim);
        }

        // let's make sure to pull aggro in any case so that archers
        // are still attacked if they are outside of the aggro range
        victim.OnAggro(entity);

        // show effects on clients
        victimCombat.RpcOnReceivedDamaged(damageDealt, damageType);

        // reset last combat time for both
        entity.lastCombatTime = NetworkTime.time;
        victim.lastCombatTime = NetworkTime.time;
    }

    // no need to instantiate damage popups on the server
    // -> calculating the position on the client saves server computations and
    //    takes less bandwidth (4 instead of 12 byte)
    [Client]
    void ShowDamagePopup(int amount, DamageType damageType)
    {
        // spawn the damage popup (if any) and set the text
        if (damagePopupPrefab != null)
        {
            // showing it above their head looks best, and we don't have to use
            // a custom shader to draw world space UI in front of the entity
            Bounds bounds = collider.bounds;
            Vector2 position = new Vector2(bounds.center.x, bounds.max.y);

            GameObject popup = Instantiate(damagePopupPrefab, position, Quaternion.identity);
            popup.GetComponent<DamageNumbersPro.DamageNumberMesh>().spamGroup = entity.name;
            if (damageType == DamageType.Normal)
                popup.GetComponent<DamageNumbersPro.DamageNumberMesh>().number = amount;
            else if (damageType == DamageType.Block)
            {
                popup.GetComponent<DamageNumbersPro.DamageNumberMesh>().number = amount;
                popup.GetComponent<DamageNumbersPro.DamageNumberMesh>().rightText = "Block";
                //popup.GetComponent<TextMesh>().text = "<i>Block!</i>";
            }
            else if (damageType == DamageType.Crit)
            {
                popup.GetComponent<DamageNumbersPro.DamageNumberMesh>().number = amount;
                popup.GetComponent<DamageNumbersPro.DamageNumberMesh>().rightText = "Crit!";
            }
        }
    }

    [Server]
    public void SetHitCount(int value)
    {
        hitCount = value;
    }

    [ClientRpc]
    public void RpcOnReceivedDamaged(int amount, DamageType damageType)
    {
        // show popup above receiver's head in all observers via ClientRpc
        ShowDamagePopup(amount, damageType);

        // call OnClientReceivedDamage event
        onClientReceivedDamage.Invoke(amount, damageType);
    }

    [ClientRpc]
    public void RpcIncrementAnimationIndex()
    {
        animationIndex = (animationIndex + 1) % 100;
    }

    [ClientRpc]
    public void RpcOnHit()
    {
        if (OnHitSFX) entity.audioSource.PlayOneShot(OnHitSFX, 2f);
    }

    [ClientRpc]
    public void RpcOnMiss()
    {
        if (OnMissSFX) entity.audioSource.PlayOneShot(OnMissSFX, 1f);
    }
}
