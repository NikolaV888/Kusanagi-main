using System;
using UnityEngine;
using Mirror;

[Serializable]
public partial struct EquipmentInfo
{
    public string requiredCategory;
    public ScriptableItemAndAmount defaultItem;
}

[RequireComponent(typeof(PlayerInventory))]
public class PlayerEquipment : Equipment
{
    [Header("Components")]
    public Player player;
    public PlayerInventory inventory;
    public SpriteRenderer spriteRenderer;
    public Texture2D transparentTexture;

    // avatar Camera is only enabled while Equipment UI is active
    [Header("Avatar")]
    public Camera avatarCamera;

    [Header("Equipment Info")]
    public SpriteMerge spriteMerge;
    public EquipmentInfo[] slotInfo = {
        new EquipmentInfo{requiredCategory="Weapon", defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Head", defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Chest", defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Legs", defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Shield", defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Shoulders", defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Hands", defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Feet", defaultItem=new ScriptableItemAndAmount()}
    };

    public override void OnStartClient()
    {
        // setup synclist callbacks on client. no need to update and show and
        // animate equipment on server
        slots.Callback += OnEquipmentChanged;

        RefreshPlayerEquipment();

    }

    void OnEquipmentChanged(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        // update the equipment
        RefreshPlayerEquipment();
    }

    public void RefreshPlayerEquipment()
    {
        MaterialPropertyBlock _propBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(_propBlock);

        

        for (int i = 0; i < slots.Count; i++)
        {
            ItemSlot slot = slots[i];
            EquipmentInfo info = slotInfo[i];

            if(slot.amount > 0) Debug.Log(i + " : " + info.requiredCategory + ", " + slot.item.name);

            // valid cateogry and valid location? otherwise don't bother
            if (info.requiredCategory != "") _propBlock.SetTexture("_Overlay" + (i + 1), slot.amount > 0 ? ((EquipmentItem)slot.item.data).spriteSheet : transparentTexture);
            //else _propBlock.SetTexture("_Overlay" + i, transparentTexture);
        }        

        spriteRenderer.SetPropertyBlock(_propBlock);
    }

    // swap inventory & equipment slots to equip/unequip. used in multiple places
    [Server]
    public void SwapInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the equipment
        if (inventory.InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= equipmentIndex && equipmentIndex < slots.Count)
        {
            // item slot has to be empty (unequip) or equipabable
            ItemSlot slot = inventory.slots[inventoryIndex];
            if (slot.amount == 0 ||
                slot.item.data is EquipmentItem itemData &&
                itemData.CanEquip(player, inventoryIndex, equipmentIndex))
            {
                // swap them
                ItemSlot temp = slots[equipmentIndex];
                slots[equipmentIndex] = slot;
                inventory.slots[inventoryIndex] = temp;
            }
        }
    }

    [Command]
    public void CmdSwapInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        SwapInventoryEquip(inventoryIndex, equipmentIndex);
    }

    [Server]
    public void MergeInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        if (inventory.InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= equipmentIndex && equipmentIndex < slots.Count)
        {
            // both items have to be valid
            // note: no 'is EquipmentItem' check needed because we already
            //       checked when equipping 'slotTo'.
            ItemSlot slotFrom = inventory.slots[inventoryIndex];
            ItemSlot slotTo = slots[equipmentIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                // make sure that items are the same type
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slotFrom.item.Equals(slotTo.item))
                {
                    // merge from -> to
                    // put as many as possible into 'To' slot
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);

                    // put back into the list
                    inventory.slots[inventoryIndex] = slotFrom;
                    slots[equipmentIndex] = slotTo;
                }
            }
        }
    }

    [Command]
    public void CmdMergeInventoryEquip(int equipmentIndex, int inventoryIndex)
    {
        MergeInventoryEquip(equipmentIndex, inventoryIndex);
    }

    [Command]
    public void CmdMergeEquipInventory(int equipmentIndex, int inventoryIndex)
    {
        if (inventory.InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= equipmentIndex && equipmentIndex < slots.Count)
        {
            // both items have to be valid
            ItemSlot slotFrom = slots[equipmentIndex];
            ItemSlot slotTo = inventory.slots[inventoryIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                // make sure that items are the same type
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slotFrom.item.Equals(slotTo.item))
                {
                    // merge from -> to
                    // put as many as possible into 'To' slot
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);

                    // put back into the list
                    slots[equipmentIndex] = slotFrom;
                    inventory.slots[inventoryIndex] = slotTo;
                }
            }
        }
    }

    // durability //////////////////////////////////////////////////////////////
    /*
    public void OnDamageDealtTo(Entity victim)
    {
        // reduce weapon durability by one each time we attacked someone
        int weaponIndex = GetEquippedWeaponIndex();
        if (weaponIndex != -1)
        {
            ItemSlot slot = slots[weaponIndex];
            slot.item.durability = Mathf.Clamp(slot.item.durability - 1, 0, slot.item.maxDurability);
            slots[weaponIndex] = slot;
        }
    }

    public void OnReceivedDamage(Entity attacker, int damage)
    {
        // reduce durability by one in each equipped item
        for (int i = 0; i < slots.Count; ++i)
        {
            if (slots[i].amount > 0)
            {
                ItemSlot slot = slots[i];
                slot.item.durability = Mathf.Clamp(slot.item.durability - 1, 0, slot.item.maxDurability);
                slots[i] = slot;
            }
        }
    }
    */

    // drag & drop /////////////////////////////////////////////////////////////
    void OnDragAndDrop_InventorySlot_EquipmentSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

        // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
        // => merge is important when dragging more arrows into an arrow slot!
        if (inventory.slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
            inventory.slots[slotIndices[0]].item.Equals(slots[slotIndices[1]].item))
        {
            CmdMergeInventoryEquip(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            CmdSwapInventoryEquip(slotIndices[0], slotIndices[1]);
        }
    }

    void OnDragAndDrop_EquipmentSlot_InventorySlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

        // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
        // => merge is important when dragging more arrows into an arrow slot!
        if (slots[slotIndices[0]].amount > 0 && inventory.slots[slotIndices[1]].amount > 0 &&
            slots[slotIndices[0]].item.Equals(inventory.slots[slotIndices[1]].item))
        {
            CmdMergeEquipInventory(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            CmdSwapInventoryEquip(slotIndices[1], slotIndices[0]); // reversed
        }
    }

    [Command]
    public void CmdUnequip(int index)
    {
        if (player.inventory.SlotsFree() <= 0) return;

        for(int i = 0; i < player.inventory.slots.Count; i++)
        {
            if(player.inventory.slots[i].amount == 0)
            {
                SwapInventoryEquip(i, index);
                return;
            }
        }
    }

    // validation
    void OnValidate()
    {
        // it's easy to set a default item and forget to set amount from 0 to 1
        // -> let's do this automatically.
        for (int i = 0; i < slotInfo.Length; ++i)
            if (slotInfo[i].defaultItem.item != null && slotInfo[i].defaultItem.amount == 0)
                slotInfo[i].defaultItem.amount = 1;
    }
}
