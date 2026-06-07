using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Clouds.SignalSystem.Editor
{
    /// <summary>
    /// Node đại diện cho một Button (sender) trong Signal Graph.
    /// </summary>
    public class SenderNode : Node
    {
        public UnityEngine.UI.Button Component { get; private set; }

        public SenderNode(UnityEngine.UI.Button component)
        {
            Component = component;
            title = component.gameObject.name;

            var objField = new ObjectField("")
            {
                value = component.gameObject,
                objectType = typeof(GameObject),
                allowSceneObjects = true
            };
            objField.SetEnabled(false);
            objField.style.width = 120;
            objField.style.height = 16;
            titleContainer.Add(objField);

            AddToClassList("sender-node");

            var collapseButton = titleContainer.Q("collapse-button");
            if (collapseButton != null) collapseButton.style.display = DisplayStyle.None;

            var port = Port.Create<Edge>(
                Orientation.Horizontal, Direction.Output,
                Port.Capacity.Multi, typeof(object));
            port.portName = "Publish";
            outputContainer.Add(port);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
