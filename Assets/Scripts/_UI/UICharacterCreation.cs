using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public partial class UICharacterCreation : MonoBehaviour
{
    public NetworkManagerMMO manager; // singleton is null until update
    public GameObject panel;
    public Button createButton;
    float requestTimer = 2f;

    void Update()
    {
        // only update while visible (after character selection made it visible)
        if (panel.activeSelf)
        {
            // still in lobby?
            if (manager.state == NetworkState.Lobby)
            {
                if (manager.charactersAvailableMsg.characters.Length > 0)
                {
                    NetworkClient.Ready();
                    NetworkClient.Send(new CharacterSelectMsg { index = 0 });
                    manager.ClearPreviews();
                    Hide();
                    return;
                }
                Show();

                if (requestTimer < 0f)
                {
                    CreateCharacter();
                    requestTimer = 2.5f;
                }
                else requestTimer -= Time.deltaTime;

                /*createButton.onClick.SetListener(() => {
                    CharacterCreateMsg message = new CharacterCreateMsg {
                        name = Random.Range(0, 999).ToString(),
                        classIndex = 1
                    };
                    NetworkClient.Send(message);
                });*/

            }
            else Hide();
        }
    }

    public void CreateCharacter()
    {
        CharacterCreateMsg message = new CharacterCreateMsg
        {
            name = Random.Range(0, 999999).ToString(),
            classIndex = 2
        };
        NetworkClient.Send(message);
    }

    public void Hide() { panel.SetActive(false); }
    public void Show() { panel.SetActive(true); }
    public bool IsVisible() { return panel.activeSelf; }
}
