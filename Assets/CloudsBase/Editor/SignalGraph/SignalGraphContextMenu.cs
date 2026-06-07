using UnityEditor;
using UnityEngine;

namespace Clouds.SignalSystem.Editor
{
    public static class SignalGraphContextMenu
    {
        [MenuItem("GameObject/Clouds/Open Signal Graph", false, 10)]
        public static void OpenSignalGraphForGameObject(MenuCommand menuCommand)
        {
            GameObject target = menuCommand.context as GameObject;
            if (target != null)
            {
                SignalGraphWindow.OpenWithPrefab(target);
            }
        }

        [MenuItem("GameObject/Clouds/Open Signal Graph", true)]
        public static bool OpenSignalGraphValidate()
        {
            // Only show menu if a GameObject is selected
            return Selection.activeGameObject != null;
        }
    }
}