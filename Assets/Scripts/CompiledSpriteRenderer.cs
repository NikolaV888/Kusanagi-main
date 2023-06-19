using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompiledSpriteRenderer : MonoBehaviour
{
    public SubAnimation[] renderers;
    public List<Sprite> sprites = new List<Sprite>();

    public SpriteRenderer sourceAnimation;
#pragma warning disable CS0109 // member does not hide accessible member
    public new SpriteRenderer renderer;
#pragma warning restore CS0109 // member does not hide accessible member

    private void Start()
    {
        RefreshSpriteRenderer();
    }

    public void RefreshSpriteRenderer()
    {

        for (int i = 0; i < renderers.Length; i++)
        {
            for (int a = 0; a < renderers[i].spritesToAnimate.Count; a++)
            {
                if (sprites.Count <= a) sprites.Add(Sprite.Create(renderers[i].spritesToAnimate[a].texture, renderers[i].spritesToAnimate[a].rect, new Vector2(0.5f, 0.35f))); //if base sprite is blank, add the new sprite

                //temporary, unity will have library to overlap textures without cycling
                for (int x = 0; x < renderers[i].spritesToAnimate[a].texture.width; x++) //cycle through width 
                {
                    for (int y = 0; y < renderers[i].spritesToAnimate[a].texture.height; y++) //cycle through height 
                    {
                        if (renderers[i].spritesToAnimate[a].texture.GetPixel(x, y).a > 0.5f) sprites[i].texture.SetPixel(x, y, renderers[i].spritesToAnimate[a].texture.GetPixel(x, y));
                    }
                }
            }
        }
        GetComponent<SpriteRenderer>().sprite = sprites[0];
    }

    void LateUpdate()
    {
        return;
        Sprite sprite = null;
        if (sprites != null)
        {
            sprite = sprites.Find(s => s.name.Contains(sourceAnimation.sprite.name));
            if (!sprite) sprite = sprites.Find(s => s.name.Contains(sourceAnimation.sprite.name.Replace("Female", "").Replace("Male", "")));
        }
        renderer.sprite = sprite;
    }
}