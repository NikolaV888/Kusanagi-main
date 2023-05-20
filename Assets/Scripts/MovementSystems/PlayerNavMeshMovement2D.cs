// we move as much of the movement code as possible into a separate component,
// so that we can switch it out with character controller movement (etc.) easily
using System;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerIndicator))]
[RequireComponent(typeof(NetworkNavMeshAgentRubberbanding2D))]
[DisallowMultipleComponent]
public class PlayerNavMeshMovement2D : NavMeshMovement2D
{
    [Header("Components")]
    public Player player;
    public PlayerIndicator indicator;
    public NetworkNavMeshAgentRubberbanding2D rubberbanding;
    public LayerMask blockableObjects;

    bool chargingChakra = false;
    bool blocking = false;

    float mouseHoldStartTime;//new for ATTACK

    public override void Reset()
    {
        // rubberbanding needs a custom reset, along with the base navmesh reset
        if (isServer)
            rubberbanding.ResetMovement();
        agent.ResetMovement();
    }

    // for 4 years since uMMORPG release we tried to detect warps in
    // NetworkNavMeshAgent/Rubberbanding. it never worked 100% of the time:
    // -> checking if dist(pos, lastpos) > speed worked well for far teleports,
    //    but failed for near teleports with dist < speed meters.
    // -> checking if speed since last update is > speed is the perfect idea,
    //    but it turns out that NavMeshAgent sometimes moves faster than
    //    agent.speed, e.g. when moving up or down a corner/stone. in fact, it
    //    sometimes moves up to 5x faster than speed, which makes warp detection
    //    hard.
    // => the ONLY 100% RELIABLE solution is to have our own Warp function that
    //    force warps the client over the network.
    // => this is extremely important for cases where players get warped behind
    //    a small door or wall. this just has to work.
    public override void Warp(Vector2 destination)
    {
        // rubberbanding needs to know about warp. this is the only 100%
        // reliable way to detect it.
        if (isServer)
            rubberbanding.RpcWarp(destination);
        agent.Warp(destination);
    }

    void Update()
    {
        // only for local player
        if (!isLocalPlayer || UIUtils.AnyInputActive()) return;

        // wasd movement allowed?
        if (player.IsMovementAllowed())
            MoveWASD();
        else LookWASD();

        //states
        chargingChakra = player.chakra.Percent() <= .99f && Input.GetKey(KeyCode.C) && !UIUtils.AnyInputActive();
        //blocking = Input.GetKey(KeyCode.G);
        blocking = Input.GetMouseButton(1);//new for right click


        //states commands
        if (Input.GetKeyDown(KeyCode.LeftShift)) player.ToggleRunning();
        if (player.blocking != blocking) player.SetBlocking(blocking);
        if (player.chargingChakra != chargingChakra) player.SetChargingChakra(chargingChakra);

        // click movement allowed?
        // (we allowed it in CASTING/STUNNED too by setting nextDestination
        //if (player.IsMovementAllowed() || player.state == "CASTING" || player.state == "STUNNED")
        //  MoveClick();

        // Flicker (remains the same)
        if (Input.GetKeyDown(KeyCode.V))
        {
            Skill skill = player.skills.skills[8];
            bool canCast = player.skills.CastCheckSelf(skill);
            if (canCast) ((PlayerSkills)player.skills).TryUse(8, true, true);
        }

        // Heavy and Light attacks based on left mouse click
        if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
        {
            mouseHoldStartTime = Time.time;
        }
        else if (Input.GetMouseButtonUp(0)) // Left mouse button released
        {
            float holdDuration = Time.time - mouseHoldStartTime;
            if (holdDuration >= 0.3f || player.combat.hitCount >= 2) // Heavy attack
            {
                Skill lightSkill = player.skills.skills[(int)player.equipment.GetEquippedWeaponType()];
                bool canCastLight = player.skills.CastCheckSelf(lightSkill);
                Skill heavySkill = player.skills.skills[(int)player.equipment.GetEquippedWeaponType() + (int)WeaponItem.weaponType.Count];
                bool canCastHeavy = player.skills.CastCheckSelf(heavySkill);
                if (canCastLight && canCastHeavy)
                    ((PlayerSkills)player.skills).TryUse((int)player.equipment.GetEquippedWeaponType() + (int)WeaponItem.weaponType.Count);
            }
            else // Light attack
            {
                Skill lightSkill = player.skills.skills[(int)player.equipment.GetEquippedWeaponType()];
                bool canCastLight = player.skills.CastCheckSelf(lightSkill);
                Skill heavySkill = player.skills.skills[(int)player.equipment.GetEquippedWeaponType() + (int)WeaponItem.weaponType.Count];
                bool canCastHeavy = player.skills.CastCheckSelf(heavySkill);
                if (canCastLight && canCastHeavy)
                    ((PlayerSkills)player.skills).TryUse((int)player.equipment.GetEquippedWeaponType());
            }
        }

    }

    [Client]
    void MoveWASD()
    {
        // don't move if currently typing in an input
        // we check this after checking h and v to save computations
        if (!UIUtils.AnyInputActive())
        {
            // get horizontal and vertical input
            // 'raw' to start moving immediately. otherwise too much delay.
            // note: no != 0 check because it's 0 when we stop moving rapidly
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            if (horizontal != 0 || vertical != 0)
            {
                // create direction, normalize in case of diagonal movement
                Vector2 direction = new Vector2(horizontal, vertical);
                if (direction.magnitude > 1) direction = direction.normalized;

                // draw direction for debugging
                Debug.DrawLine(transform.position, transform.position + (Vector3)direction, Color.green, 0, false);

                // clear indicator if there is one, and if it's not on a target
                // (simply looks better)
                if (direction != Vector2.zero)
                    indicator.ClearIfNoParent();

                // cancel path if we are already doing click movement, otherwise
                // we will slide
                agent.ResetMovement();

                // note: SetSpeed() already sets agent.speed to player.speed
                //Collider2D hit = Physics2D.OverlapCircle((Vector2)transform.position + (direction / 3f), .05f, blockableObjects);
                //if (!hit) 
                    agent.velocity = direction * agent.speed; //this is a bad idea but obstacle navigation has issues with warping
                //Navigate(((Vector2Int)Vector3Int.RoundToInt(transform.position)) + ((Vector2Int)Vector3Int.RoundToInt(direction)), 0);

                // clear requested skill in any case because if we clicked
                // somewhere else then we don't care about it anymore
                player.useSkillWhenCloser = -1;
            }
        }

    }
    [Client]
    void LookWASD()
    {
        // don't move if currently typing in an input
        // we check this after checking h and v to save computations
        if (!UIUtils.AnyInputActive())
        {
            // get horizontal and vertical input
            // 'raw' to start moving immediately. otherwise too much delay.
            // note: no != 0 check because it's 0 when we stop moving rapidly
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            if (horizontal != 0 || vertical != 0)
            {
                // create direction, normalize in case of diagonal movement
                Vector2 direction = new Vector2(horizontal, vertical);
                if (direction.magnitude > 1) direction = direction.normalized;

                player.lookDirection = direction;
            }
        }

    }

    [Client]
    void MoveClick()
    {
        // click raycasting if not over a UI element & not pinching on mobile
        // note: this only works if the UI's CanvasGroup blocks Raycasts
        if (Input.GetMouseButtonDown(0) &&
            !Utils.IsCursorOverUserInterface() &&
            Input.touchCount <= 1)
        {
            Camera cam = Camera.main;

            // cast a 3D ray from the camera towards the 2D scene.
            // Physics2D.Raycast isn't made for that, we use GetRayIntersection.
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // raycast with local player ignore option
            RaycastHit2D hit = player.localPlayerClickThrough
                               ? Utils.Raycast2DWithout(ray, gameObject)
                               : Physics2D.GetRayIntersection(ray);

            // clicked a movement target, not an entity?
            if (hit.transform == null || hit.transform.GetComponent<Entity>() == null)
            {
                // set indicator and navigate to the nearest walkable
                // destination. this prevents twitching when destination is
                // accidentally in a room without a door etc.
                Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector2 bestDestination = NearestValidDestination(worldPos);
                indicator.SetViaPosition(bestDestination);

                // casting or stunned? then set pending destination
                if (player.state == "CASTING" || player.state == "STUNNED")
                {
                    player.pendingDestination = bestDestination;
                    player.pendingDestinationValid = true;
                }
                // otherwise navigate there
                else Navigate(bestDestination, 0);
            }
        }
    }

    // validation //////////////////////////////////////////////////////////////
    void OnValidate()
    {
        // make sure that the NetworkNavMeshAgentRubberbanding component is
        // ABOVE this component, so that it gets updated before this one.
        // -> otherwise it overwrites player's WASD velocity for local player
        //    hosts
        // -> there might be away around it, but a warning is good for now
        Component[] components = GetComponents<Component>();
        if (Array.IndexOf(components, GetComponent<NetworkNavMeshAgentRubberbanding2D>()) >
            Array.IndexOf(components, this))
            Debug.LogWarning(name + "'s NetworkNavMeshAgentRubberbanding2D component is below the PlayerNavMeshMovement2D component. Please drag it above the Player component in the Inspector, otherwise there might be WASD movement issues due to the Update order.");
    }
}
