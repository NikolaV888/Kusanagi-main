// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;

public partial class UISkills : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.R;
    public GameObject panel;
    public RectTransform panelRectTransform;
    public UISkillSlot slotPrefab;
    public Transform content;

    private void Awake()
    {
        panelRectTransform = panel.GetComponent<RectTransform>();
    }

    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);

            // only update the panel if it's active
            if (panel.activeSelf)
            {
                int usableJutsu = 0;
                for (int i = 0; i < player.skills.skills.Count; i++)
                {
                    if(player.skills.skills[i].canBeHotbarred) usableJutsu++;
                }

                int slotCount = usableJutsu + (4 - usableJutsu % 4);
                panelRectTransform.sizeDelta = content.GetComponent<RectTransform>().sizeDelta + new Vector2(0,20);

                // instantiate/destroy enough slots
                // (we only care about non status skills)
                UIUtils.BalancePrefabs(slotPrefab.gameObject, slotCount, content);

                int negativeIncrement = 0;
                // refresh all
                for (int i = 0; i < player.skills.skills.Count; ++i)
                {
                    UISkillSlot slot = content.GetChild(i - negativeIncrement).GetComponent<UISkillSlot>();
                    Skill skill = player.skills.skills[i];

                    if (skill.canBeHotbarred)
                    {
                        bool isPassive = skill.data is PassiveSkill;

                        // set state
                        slot.dragAndDropable.name = i.ToString();
                        slot.dragAndDropable.dragable = skill.level > 0 && !isPassive;

                        // can we cast it? checks mana, cooldown etc.
                        bool canCast = player.skills.CastCheckSelf(skill);

                        // if movement does NOT support navigation then we need to
                        // check distance too. otherwise distance doesn't matter
                        // because we can navigate anywhere.
                        if (!player.movement.CanNavigate())
                            canCast &= player.skills.CastCheckDistance(skill, out Vector2 _);

                        // click event
                        slot.button.interactable = skill.level > 0 &&
                                                   !isPassive &&
                                                   canCast;

                        int icopy = i;
                        slot.button.onClick.SetListener(() =>
                        {
                            // try use the skill or walk closer if needed
                            ((PlayerSkills)player.skills).TryUse(icopy);
                        });

                        // image
                        if (skill.level > 0)
                        {
                            slot.image.color = Color.white;
                            slot.image.sprite = skill.image;
                            slot.image.enabled = true;
                        }
                    }
                    else
                    {
                        negativeIncrement++;
                        slot.image.enabled = false;
                    }
                }

                //empty slots
                for (int i = player.skills.skills.Count - negativeIncrement; i < slotCount; ++i)
                {
                    UISkillSlot slot = content.GetChild(i).GetComponent<UISkillSlot>();
                    slot.image.enabled = false;
                }
            }
        }
        else panel.SetActive(false);
    }
}
