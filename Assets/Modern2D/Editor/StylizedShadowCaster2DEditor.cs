using UnityEditor;
using UnityEngine;

namespace Modern2D
{

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StylizedShadowCaster2D))]
    public class StylizedShadowCaster2DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("CreateShadows"))
                foreach (StylizedShadowCaster2D caster in targets)
                    caster.CreateShadow();
            if (GUILayout.Button("RebuildShadows"))
                foreach (StylizedShadowCaster2D caster in targets)
                    caster.RebuildShadow();
        }
    }
#endif
}