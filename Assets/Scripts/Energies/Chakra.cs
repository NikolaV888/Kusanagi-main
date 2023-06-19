using UnityEngine;

// inventory, attributes etc. can influence max mana
public interface IChakraBonus
{
    int GetChakraBonus(int baseMana);
    int GetChakraRecoveryBonus();
}

[RequireComponent(typeof(Level))]
[DisallowMultipleComponent]
public class Chakra : Energy
{
    public Level level;
    public LinearInt baseChakra = new LinearInt{baseValue=100};
    public int baseRecoveryRate = 1;

    // cache components that give a bonus (attributes, inventory, etc.)
    // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
    IChakraBonus[] _bonusComponents;
    IChakraBonus[] bonusComponents =>
        _bonusComponents ?? (_bonusComponents = GetComponents<IChakraBonus>());

    // calculate max
    public override int max
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            int baseThisLevel = baseChakra.Get(level.current);
            foreach (IChakraBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetChakraBonus(baseThisLevel);
            return baseThisLevel + bonus;
        }
    }

    public override int recoveryRate
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            foreach (IChakraBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetChakraRecoveryBonus();
            return baseRecoveryRate + bonus;
        }
    }
}