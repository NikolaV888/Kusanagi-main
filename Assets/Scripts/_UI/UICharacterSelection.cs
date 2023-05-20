// Simple character selection list. The charcter prefabs are known, so we could
// easily show 3D models, stats, etc. too.
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public partial class UICharacterSelection : MonoBehaviour
{
    public UICharacterCreation uiCharacterCreation;
    public UIConfirmation uiConfirmation;
    public NetworkManagerMMO manager; // singleton is null until update
    public GameObject panel;
    public Button startButton;
    public Button deleteButton;
    public Button createButton;
    public Button quitButton;

    void Update()
    {
        // show while in lobby and while not creating a character
        if (manager.state == NetworkState.Lobby && !uiCharacterCreation.IsVisible())
        {
            panel.SetActive(true);

            // characters available message received already?
            if (manager.charactersAvailableMsg.characters != null)
            {
                CharactersAvailableMsg.CharacterPreview[] characters = manager.charactersAvailableMsg.characters;

                if(characters.Length == 0)
                {
                    panel.SetActive(false);
                    uiCharacterCreation.Show();
                }
                else
                {
                    /*NetworkClient.Ready();
                    NetworkClient.Send(new CharacterSelectMsg { index = 0 });
                    manager.ClearPreviews();
                    panel.SetActive(false);*/
                }
            }
        }
        else panel.SetActive(false);
    }
}
