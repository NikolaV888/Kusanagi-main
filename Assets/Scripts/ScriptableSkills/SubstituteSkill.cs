using UnityEngine;

[CreateAssetMenu(menuName = "Jutsu/Substitute", order = 999)]
public class SubstituteSkill : BuffSkill
{

    public override bool CheckTarget(Entity caster)
    {
        return true;
    }

    // (has corrected target already)
    public override bool CheckDistance(Entity caster, int skillLevel, out Vector2 destination)
    {
        // check distance to corrected target (without setting the target)
        // this way we can call CheckDistance without setting the corrected
        // target in CheckTarget first. this is needed for skillbar.interactable
        Entity target = caster;

        // target still around?
        if (target != null)
        {
            destination = target.collider.ClosestPointOnBounds(caster.transform.position);
            return Utils.ClosestDistance(caster.collider, target.collider) <= castRange.Get(skillLevel);
        }
        destination = caster.transform.position;
        return false;
    }

    // (has corrected target already)
    public override void Apply(Entity caster, int skillLevel, Vector2 direction)
    {
        Debug.Log("Sub Apply");
        // note: caster already has the corrected target because we returned it in StartCast
        // can't buff dead people
        if (caster.health.current > 0)
        {
            Debug.Log("Sub Apply 2");
            // add buff or replace if already in there
            caster.skills.AddOrRefreshBuff(new Buff(this, skillLevel, caster.transform.position));
            Debug.Log("Sub Apply 3");

            // show effect on target
            SpawnEffect(caster, caster);
            Debug.Log("Sub Apply 4");
        }
    }
}
