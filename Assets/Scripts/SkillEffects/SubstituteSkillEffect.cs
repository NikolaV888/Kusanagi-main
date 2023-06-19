// A simple skill effect that follows the target until it ends.
// -> Can be used for buffs.
//
// Note: Particle Systems need Simulation Space = Local for it to work.
using UnityEngine;
using Mirror;

public class SubstituteSkillEffect : SkillEffect
{
    float timeLeft = 5f;

    private void Start()
    {
        
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft > 0) return;

        if (isServer) NetworkServer.Destroy(gameObject);
    }
}
