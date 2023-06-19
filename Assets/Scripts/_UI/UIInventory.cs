// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;

public partial class UIInventory : MonoBehaviour
{
    public static UIInventory singleton;
    public KeyCode hotKey = KeyCode.I;
    public GameObject panel;
    public UIInventorySlot slotPrefab;
    public UIInventorySlot lockedSlotPrefab;
    public Transform content;
    //public Text goldText;

    [Header("Durability Colors")]
    public Color brokenDurabilityColor = Color.red;
    public Color lowDurabilityColor = Color.magenta;
    [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;

    public UIInventory()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }

    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);

            // only update the panel if it's active
            if (panel.activeSelf)
            {
                if(content.childCount == 0)
                {
                    for (int i = 0; i < player.inventory.Size(); ++i)
                    {
                        GameObject slot = GameObject.Instantiate(slotPrefab.gameObject);
                        slot.transform.SetParent(content, false);
                    }

                    while(content.childCount < 40)
                    {
                        GameObject slot = GameObject.Instantiate(lockedSlotPrefab.gameObject);
                        slot.transform.SetParent(content, false);
                    }
                }

                // instantiate/destroy enough slots
                //UIUtils.BalancePrefabs(slotPrefab.gameObject, player.inventory.slots.Count, content);
                /*if(player.isPremium) UIUtils.BalancePrefabs(slotPrefab.gameObject, 10, content);
                else UIUtils.BalancePrefabs(lockedSlotPrefab.gameObject, 10, content);*/

                // refresh all items
                for (int i = 0; i < 40; ++i)
                {
                    UIInventorySlot slot = content.GetChild(i).GetComponent<UIInventorySlot>();
                    slot.dragAndDropable.name = i.ToString(); // drag and drop index

                    if (i >= player.inventory.slots.Count)
                    {
                        Debug.Log("Locked slot " + i);
                        slot.button.onClick.RemoveAllListeners();
                        slot.tooltip.enabled = false;
                        slot.dragAndDropable.dragable = false;
                        slot.dragAndDropable.dropable = false;
                        slot.image.color = Color.clear;
                        slot.image.sprite = null;
                        slot.cooldownCircle.fillAmount = 0;
                        slot.amountOverlay.SetActive(false);
                    }
                    else
                    {
                        ItemSlot itemSlot = player.inventory.slots[i];

                        if (itemSlot.amount > 0)
                        {
                            // refresh valid item
                            int icopy = i; // needed for lambdas, otherwise i is Count
                            slot.button.onClick.SetListener(() => {
                                if (itemSlot.item.data is UsableItem usable &&
                                    usable.CanUse(player, icopy))
                                    player.inventory.CmdUseItem(icopy);
                            });
                            // only build tooltip while it's actually shown. this
                            // avoids MASSIVE amounts of StringBuilder allocations.
                            slot.tooltip.enabled = true;
                            if (slot.tooltip.IsVisible())
                                slot.tooltip.text = itemSlot.ToolTip();
                            slot.dragAndDropable.dragable = true;

                            // use durability colors?
                            /*if (itemSlot.item.maxDurability > 0)
                            {
                                if (itemSlot.item.durability == 0)
                                    slot.image.color = brokenDurabilityColor;
                                else if (itemSlot.item.DurabilityPercent() < lowDurabilityThreshold)
                                    slot.image.color = lowDurabilityColor;
                                else
                                    slot.image.color = Color.white;
                            }
                            else*/
                            slot.image.color = Color.white; // reset for no-durability items
                            slot.image.sprite = itemSlot.item.image;

                            // cooldown if usable item
                            if (itemSlot.item.data is UsableItem usable2)
                            {
                                float cooldown = player.GetItemCooldown(usable2.cooldownCategory);
                                slot.cooldownCircle.fillAmount = usable2.cooldown > 0 ? cooldown / usable2.cooldown : 0;
                            }
                            else slot.cooldownCircle.fillAmount = 0;
                            slot.amountOverlay.SetActive(itemSlot.amount > 1);
                            slot.amountText.text = itemSlot.amount.ToString();
                        }
                        else
                        {
                            // refresh invalid item
                            slot.button.onClick.RemoveAllListeners();
                            slot.tooltip.enabled = false;
                            slot.dragAndDropable.dragable = false;
                            slot.image.color = Color.clear;
                            slot.image.sprite = null;
                            slot.cooldownCircle.fillAmount = 0;
                            slot.amountOverlay.SetActive(false);
                        }
                    }
                    
                }

                // gold
                //goldText.text = player.gold.ToString();
                
            }
        }
        else panel.SetActive(false);
    }
}
