
using System.Collections.Generic;
using UnityEngine;

public class SubAnimation : MonoBehaviour
{
    public SpriteRenderer sourceAnimation;
#pragma warning disable CS0109 // member does not hide accessible member
    public new SpriteRenderer renderer;
#pragma warning restore CS0109 // member does not hide accessible member
    public List<Sprite> spritesToAnimate;

    void LateUpdate()
    {
        Sprite sprite = null;
        if (spritesToAnimate != null)
        {
            sprite = spritesToAnimate.Find(s => s.name.EndsWith(sourceAnimation.sprite.name));
            if (!sprite) sprite = spritesToAnimate.Find(s => s.name.EndsWith(sourceAnimation.sprite.name.Replace("Female", "").Replace("Male", "")));
        }
        renderer.sprite = sprite;
    }
}
