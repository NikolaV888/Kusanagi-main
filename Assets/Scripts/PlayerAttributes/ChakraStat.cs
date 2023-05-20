// Strength Attribute that grants extra health.
// IMPORTANT: SyncMode=Observers needed to show other player's health correctly!
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ChakraStat : PlayerAttribute, IChakraBonus
{
    public int chakraGainedPerPoint = 5;

    public int GetChakraBonus(int baseMana) => (value * chakraGainedPerPoint);

    public int GetChakraRecoveryBonus() => 0;
}
