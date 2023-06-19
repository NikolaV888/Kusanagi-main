// Strength Attribute that grants extra health.
// IMPORTANT: SyncMode=Observers needed to show other player's health correctly!
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthStat : PlayerAttribute, IHealthBonus
{
    public int healthGainedPerPoint = 5;

    public int GetHealthBonus(int baseHealth) =>
        (value * healthGainedPerPoint);

    public int GetHealthRecoveryBonus() => 0;
}
