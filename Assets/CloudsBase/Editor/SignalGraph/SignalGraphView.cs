using System.Collections.Generic;
using System.Linq;
using Clouds.UI.Animation;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Clouds.SignalSystem.Editor
{
    /// <summary>
    /// Visualizes Button (senders) and UIAnimationContainer (receivers) in the scene/prefab.
    /// Signal connections are code-defined via SignalBus — edges drawn here are
    /// informational only and are NOT written back to any serialized field.
    /// </summary>
    public class SignalGraphView : GraphView
    {
        public SignalGraphView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentZoomer());

            style.flexGrow = 1;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
            => ports.ToList().Where(p => p.direction != startPort.direction && p.node != startPort.node).ToList();

        public void PopulateFromPrefab(GameObject prefab)
        {
            DeleteElements(graphElements);

            IEnumerable<UnityEngine.UI.Button> senders;
            IEnumerable<UIAnimationContainer> receivers;

            if (prefab != null)
            {
                senders   = prefab.GetComponentsInChildren<UnityEngine.UI.Button>(true);
                receivers = prefab.GetComponentsInChildren<UIAnimationContainer>(true);
            }
            else
            {
                senders   = Object.FindObjectsOfType<UnityEngine.UI.Button>(true);
                receivers = Object.FindObjectsOfType<UIAnimationContainer>(true);
            }

            float yS = 50f;
            foreach (var sender in senders)
            {
                var node = new SenderNode(sender);
                AddElement(node);
                node.SetPosition(new Rect(100, yS, 260, 80));
                yS += 120f;
            }

            float yR = 50f;
            foreach (var receiver in receivers)
            {
                var node = new ReceiverNode(receiver);
                AddElement(node);
                node.SetPosition(new Rect(500, yR, 260, 80));
                yR += 120f;
            }

            schedule.Execute(() => FrameAll()).ExecuteLater(100);
        }
    }
}
