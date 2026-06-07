using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Clouds.SignalSystem.Editor
{
    public class SignalGraphWindow : EditorWindow
    {
        private SignalGraphView _graphView;
        private GameObject _currentPrefab;
        private ObjectField _prefabField;

        [MenuItem("Clouds/Signal Graph Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<SignalGraphWindow>("Signal Graph");
            window.titleContent = new GUIContent("Signal Graph");
            window.Show();

            // Auto-populate with scene objects if no specific target is set
            if (window._graphView != null && window._currentPrefab == null)
                window._graphView.PopulateFromPrefab(null);
        }

        public static void OpenWithPrefab(GameObject prefab)
        {
            var window = GetWindow<SignalGraphWindow>("Signal Graph");
            window.titleContent = new GUIContent("Signal Graph");
            window._currentPrefab = prefab;
            
            window.Show();
            
            // Use delay call to ensure the window and graph view are fully initialized
            EditorApplication.delayCall += () => {
                if (window != null && window._graphView != null)
                {
                    if (window._prefabField != null) 
                        window._prefabField.value = prefab;
                    
                    window._graphView.PopulateFromPrefab(prefab);
                }
            };
        }

        private void OnEnable()
        {
            GenerateToolbar();
            ConstructGraphView();
        }

        private void ConstructGraphView()
        {
            _graphView = new SignalGraphView
            {
                name = "Signal Graph View"
            };
            _graphView.style.flexGrow = 1;
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            _prefabField = new ObjectField("Prefab Target")
            {
                objectType = typeof(GameObject),
                allowSceneObjects = true
            };

            if (_currentPrefab != null) _prefabField.value = _currentPrefab;

            _prefabField.RegisterValueChangedCallback(evt =>
            {
                _currentPrefab = (GameObject)evt.newValue;
                if (_currentPrefab != null)
                {
                    _graphView.PopulateFromPrefab(_currentPrefab);
                }
            });

            toolbar.Add(_prefabField);

            var refreshButton = new Button(() =>
            {
                if (_currentPrefab != null)
                    _graphView.PopulateFromPrefab(_currentPrefab);
            }) { text = "Refresh" };
            
            toolbar.Add(refreshButton);

            rootVisualElement.Add(toolbar);
        }
    }
}