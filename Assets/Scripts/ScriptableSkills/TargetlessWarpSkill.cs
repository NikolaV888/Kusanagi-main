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
        // log the look direction and warp distance
        Debug.Log("Look direction: " + caster.lookDirection);
        Debug.Log("Warp distance: " + warpDistance);

        Vector2 raycastEnd = (Vector2)caster.transform.position + (caster.lookDirection * warpDistance);

        // print the start and end points of the raycast
        Debug.Log("Raycast start: " + caster.transform.position);
        Debug.Log("Raycast end: " + raycastEnd);

        RaycastHit2D raycast = Physics2D.Linecast(caster.transform.position, raycastEnd, warpBlockingLayers);

        if (raycast.collider)
        {
            // print the point and the name of the collider that the raycast hits
            Debug.Log("Raycast hit: " + raycast.point);
            Debug.Log("Warp Blocked By " + raycast.collider.name);

            Debug.Log("Attempting to warp to: " + raycast.point);
            caster.movement.Warp(raycast.point);
            Debug.Log("Warp command completed");
        }
        else
        {
            // print the point where the raycast ends
            Debug.Log("Raycast end: " + raycastEnd);

            Debug.Log("Attempting to warp to: " + raycastEnd);
            caster.movement.Warp(raycastEnd);
            Debug.Log("Warp command completed");
        }
    }
}