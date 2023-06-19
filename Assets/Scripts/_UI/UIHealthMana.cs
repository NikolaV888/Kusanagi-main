using UnityEngine;
using UnityEngine.UI;

public partial class UIHealthMana : MonoBehaviour
{
    public GameObject panel;
    public Slider healthSlider;
    public Text healthStatus;
    public Slider manaSlider;
    public Text manaStatus;

    public GameObject flickerPoint;
    public GameObject flickerPoint2;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            panel.SetActive(true);

            healthSlider.value = player.health.Percent();
            healthStatus.text = player.health.current + " / " + player.health.max;

            manaSlider.value = player.chakra.Percent();
            manaStatus.text = player.chakra.current + " / " + player.chakra.max;

            flickerPoint.SetActive(player.flickerEnergy.current >= 200);
            flickerPoint2.SetActive(player.flickerEnergy.current >= 400);
        }
        else panel.SetActive(false);
    }
}
