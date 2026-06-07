using Clouds.UI.Animation;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Clouds.SignalSystem.Editor
{
    /// <summary>
    /// Node đại diện cho một UIAnimationContainer (panel/receiver) trong Signal Graph.
    /// </summary>
    public class ReceiverNode : Node
    {
        public UIAnimationContainer Component { get; private set; }

        public ReceiverNode(UIAnimationContainer component)
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

            AddToClassList("receiver-node");

            var collapseButton = titleContainer.Q("collapse-button");
            if (collapseButton != null) collapseButton.style.display = DisplayStyle.None;

            var port = Port.Create<Edge>(
                Orientation.Horizontal, Direction.Input,
                Port.Capacity.Multi, typeof(object));
            port.portName = "Subscribe";
            inputContainer.Add(port);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
