// Quick slash in front of us that hits anything standing there.
//
// => Useful for hack & slash / action based combat skills/games without target.
//    (set one of the skillbar slots to SPACEBAR key for the ultimate effect)
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName="Jutsu/Slash Damage", order=999)]
public class SlashDamageSkill : DamageSkill
{
    public bool incrementHitCount = false;
    public int requiredHitCount = 0;
    [Header("Slash SFX")]
    public List<AudioClip> OnHitSound = new List<AudioClip>();
    public List<AudioClip> OnMissSound = new List<AudioClip>();

    public override void OnCastFinished(Entity caster)
    {
        if (OnHitSound.Count > 0) caster.combat.OnHitSFX = OnHitSound[Random.Range(0, OnHitSound.Count)];
        if (OnMissSound.Count > 0) caster.combat.OnMissSFX = OnMissSound[Random.Range(0, OnMissSound.Count)];
    }

    public override bool CheckTarget(Entity caster)
    {
        // no target necessary, but still set to self so that LookAt(target)
        // doesn't cause the player to look at a target that doesn't even matter
        //caster.target = caster;
        return true;
    }

    public override bool CheckDistance(Entity caster, int skillLevel, out Vector2 destination)
    {
        // can cast anywhere
        destination = (Vector2)caster.transform.position + caster.lookDirection;
        return true;
    }

    public override void Apply(Entity caster, int skillLevel, Vector2 direction)
    {
        // cast a box or circle into look direction and try to attack anything
        // that is attackable
        float range = castRange.Get(skillLevel);
        Vector2 center = (Vector2)caster.transform.position + ((direction * range) / 6f);
        Vector2 size = new Vector2(range, range);
        if (caster.movement.IsMoving()) size *= 1.1f;
        Debug.DrawLine(new Vector2(center.x - (size.x/2f), center.y), new Vector2(center.x + (size.x / 2f), center.y), Color.red, 10f);
        Debug.DrawLine(new Vector2(center.x, center.y - (size.y / 2f)), new Vector2(center.x, center.y + (size.y / 2f)), Color.red, 10f);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0);
        bool miss = true;
        foreach (Collider2D co in colliders)
        {
            Entity candidate = co.GetComponentInParent<Entity>();
            //Debug.Log("Can Attack " + co.gameObject.name + " ? " + caster.CanAttack(candidate));
            if (candidate != null && caster.CanAttack(candidate))
            {
                //Vector2 lookDirection = (candidate.transform.position - caster.transform.position).normalized;
                //if (Mathf.Abs(lookDirection.x) > Mathf.Abs(lookDirection.y)) lookDirection.y = 0;
                //if (Mathf.Abs(lookDirection.x) < Mathf.Abs(lookDirection.y)) lookDirection.x = 0;

                //caster.RpcSetLookDirection(lookDirection);

                // deal damage directly with base damage + skill damage
                if(caster.combat.hitCount >= requiredHitCount) caster.combat.DealDamageAt(candidate, caster.combat.damage + damage.Get(skillLevel), stunChance.Get(skillLevel), stunTime.Get(skillLevel), pushback.Get(skillLevel), pushforward.Get(skillLevel));
                else caster.combat.DealDamageAt(candidate, caster.combat.damage + damage.Get(skillLevel), 0, 0, 0, 0);
                miss = false;

                if (targetVFX)
                {
                    GameObject go = Instantiate(targetVFX.gameObject, candidate.skills.effectMount.position, candidate.skills.effectMount.rotation);
                    NetworkServer.Spawn(go);
                }
                //if (caster.auraGameObject) caster.auraGameObject.SetActive(true);
            }
            else
            {
                //caster.movement.Warp((Vector2)caster.transform.position + (caster.lookDirection * pushforward.Get(skillLevel) * .25f));
                //if (caster.auraGameObject) caster.auraGameObject.SetActive(false);
            }
        }

        if (!miss && incrementHitCount)
        {
            caster.combat.SetHitCount(caster.combat.hitCount + 1);
        }
        else
        {
            caster.combat.SetHitCount(0);
        }

        if (!miss && OnHitSound.Count > 0) caster.combat.RpcOnHit();
        if (miss && OnMissSound.Count > 0) caster.combat.RpcOnMiss();

        if (casterVFX)
        {
            GameObject go = Instantiate(casterVFX.gameObject, caster.skills.effectMount.position, caster.skills.effectMount.rotation);
            NetworkServer.Spawn(go);
        }
    }
}
