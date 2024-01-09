using AI.Nodes;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace AI
{
    public class BehaviourTreeEditor : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset         m_VisualTreeAsset = default;

        private BehaviourTreeView  m_view;

        [MenuItem("AICourse/Behaviour Tree Editor")]
        public static void ShowBehaviourTreeEditor()
        {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("BT");
        }

        [OnOpenAsset]
        public static bool OpenAITree(int instanceID, int line)
        {
            BehaviourTree tree = EditorUtility.InstanceIDToObject(instanceID) as BehaviourTree;
            Node node = EditorUtility.InstanceIDToObject(instanceID) as Node;

            if (tree == null && node != null)
            {
                tree = AssetDatabase.LoadAssetAtPath<BehaviourTree>(AssetDatabase.GetAssetPath(instanceID));
            }

            if (tree != null)
            {
                BehaviourTreeEditor treeWindow = GetWindow<BehaviourTreeEditor>();
                Selection.activeObject = tree;
                treeWindow.titleContent = new GUIContent(tree.name);
                return true;
            }

            return false;
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= PlayModeChanged;
            EditorApplication.playModeStateChanged += PlayModeChanged;
        }

        private void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= PlayModeChanged;
        }

        private void PlayModeChanged(PlayModeStateChange obj)
        {
            CreateGUI();
            m_view?.PopulateView(null);
            titleContent = new GUIContent("BT");
        }

        public void CreateGUI()
        {
            if (m_view == null)
            {
                VisualElement root = rootVisualElement;
                m_VisualTreeAsset.CloneTree(root);
                m_view = root.Query<BehaviourTreeView>();
            }

            OnSelectionChange();
        }

        private void OnSelectionChange()
        {
            // selected a tree?
            BehaviourTree tree = Selection.activeObject as BehaviourTree;

            // runtime brain?
            if (tree == null &&
                Application.isPlaying &&
                Selection.activeGameObject != null)
            {
                Brain brain = Selection.activeGameObject.GetComponent<Brain>();
                tree = brain?.Tree;
            }

            // got a tree?
            if (tree != null && (AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()) || Application.isPlaying))
            {
                m_view.PopulateView(tree);
                titleContent = new GUIContent(tree.name);
            }
        }

        private void OnInspectorUpdate()
        {
            m_view?.UpdateNodeStates();
        }
    }
}