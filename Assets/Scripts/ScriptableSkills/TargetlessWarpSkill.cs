// for targetless projectiles that are fired into a general direction.
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName = "uMMORPG Skill/Targetless Warp", order = 999)]
public class TargetlessWarpSkill : ScriptableSkill
{
    public float warpDistance;
    public LayerMask warpBlockingLayers;

    public override bool CheckSelf(Entity caster, int skillLevel)
    {
        // check base and ammo
        return base.CheckSelf(caster, skillLevel);
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
        RaycastHit2D raycast = Physics2D.Linecast(caster.transform.position, (Vector2)caster.transform.position + (caster.lookDirection * warpDistance), warpBlockingLayers);

        if (raycast.collider)
        {
            caster.movement.Warp(raycast.point);
            Debug.Log("Warp Blocked By " + raycast.collider.name);
        }
        else caster.movement.Warp((Vector2)caster.transform.position + (caster.lookDirection * warpDistance));
    }
}
