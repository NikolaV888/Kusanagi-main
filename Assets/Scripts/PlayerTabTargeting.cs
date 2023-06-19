using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerIndicator))]
[DisallowMultipleComponent]
public class PlayerTabTargeting : NetworkBehaviour
{
    [Header("Components")]
    public Player player;
    public PlayerIndicator indicator;

    [Header("Targeting")]
    public KeyCode enemyKey = KeyCode.Tab;
    public KeyCode allyKey = KeyCode.Tab;

    void Update()
    {
        // only for local player
        if (!isLocalPlayer) return;

        // in a state where tab targeting is allowed?
        if (player.state == "IDLE" ||
            player.state == "MOVING" ||
            player.state == "CASTING" ||
            player.state == "STUNNED" || true)
        {
            // key pressed?
            if (Input.GetKeyDown(enemyKey))
                TargetNearestEnemy();
        }
    }

    [Client]
    void TargetNearestEnemy()
    {
        List<Entity> availableTargets = NetworkClient.spawned.Values
            .Select(ni => ni.GetComponent<Entity>())
            .Where(m => m != null && m.health.current > 0 && m != player && m != m.target)
            .ToList();
        List<Entity> sorted = availableTargets.OrderBy(m => Vector3.Distance(transform.position, m.transform.position)).ToList();

        // target nearest one
        if (sorted.Count > 0)
        {
            indicator.SetViaParent(sorted[0].transform);
            player.CmdSetTarget(sorted[0].netIdentity);
        }
    }
}
