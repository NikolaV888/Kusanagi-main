using UnityEngine;

[RequireComponent(typeof(Level))]
[DisallowMultipleComponent]
public class FlickerEnergy : Energy
{
    public Level level;

    public LinearInt baseFlickerPoints = new LinearInt{baseValue=100};
    public int baseRecoveryRate = 1;

    // calculate max
    public override int max
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            //int bonus = 0;
            int baseThisLevel = baseFlickerPoints.Get(level.current);
            return baseThisLevel;
        }
    }

    public override int recoveryRate
    {
        get
        {
            return baseRecoveryRate;
        }
    }
}