using UnityEngine;
using UnityEditor;

namespace Modern2D
{

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    public class SetLayerAfterDemoSceneLoads : MonoBehaviour
    {
        [SerializeField] public string layer = "";
    }
#endif
}