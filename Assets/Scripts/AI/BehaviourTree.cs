using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI.Nodes;

namespace AI
{
    [CreateAssetMenu(fileName = "BehaviourTree", menuName = "AICourse/BehaviourTree")]
    public class BehaviourTree : ScriptableObject
    {
        [SerializeField]
        public List<Node>   m_nodes = new List<Node>();

        [SerializeField]
        public Node         m_root;

        private Brain       m_targetBrain;
        private Node.State  m_currentState = Node.State.Running;
        private Blackboard  m_blackboard;

        #region Properties

        public Brain TargetBrain => m_targetBrain;

        public GameObject TargetGameObject => m_targetBrain.gameObject;

        public Blackboard Blackboard => m_blackboard;

        public Node.State CurrentState => m_currentState;

        #endregion

        public void StartTree(Brain brain)
        {
            m_targetBrain = brain;
            m_blackboard = new Blackboard();

            foreach (Node node in m_nodes)
            {
                node.OnTreeStart(this);
            }
        }

        public Node.State Update()
        {
            if (m_root != null && m_root.m_state == Node.State.Running)
            {
                m_currentState = m_root.Update();
            }

            return m_currentState;
        }

        public Node CreateNode(System.Type type)
        {
            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "State Created");
            #endif

            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            m_nodes.Add(node);

            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
            UnityEditor.Undo.RegisterCreatedObjectUndo(node, "State Created");
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            #endif

            return node;
        }

        public void DeleteNode(Node node)
        {
            if (node == null)
            {
                return;
            }

            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "State Deleted");
            #endif

            m_nodes.Remove(node);

            #if UNITY_EDITOR
            UnityEditor.Undo.DestroyObjectImmediate(node);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            #endif
        }

        public void AddChild(Node parent, Node child)
        {
            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(parent, "Node Child Added");
            #endif

            if (parent is Root root)
            {
                root.m_child = child;
            }
            else if (parent is DecoratorNode decorator)
            {
                decorator.m_child = child;
            }
            else if (parent is CompositeNode composite &&
                     !composite.m_children.Contains(child))
            {
                composite.m_children.Add(child);
            }

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(parent);
            #endif
        }

        public void RemoveChild(Node parent, Node child)
        {
            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(parent, "Node Child Removed");
            #endif

            if (parent is Root root)
            {
                root.m_child = null;
            }
            else if (parent is DecoratorNode decorator)
            {
                decorator.m_child = null;
            }
            else if (parent is CompositeNode composite &&
                     composite.m_children.Contains(child))
            {
                composite.m_children.Remove(child);
            }

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(parent);
            #endif
        }

        public List<Node> GetChildren(Node parent)
        {
            List<Node> children = new List<Node>();

            if (parent is Root root)
            {
                children.Add(root.m_child);
            }
            else if (parent is DecoratorNode decorator)
            {
                children.Add(decorator.m_child);
            }
            else if (parent is CompositeNode composite)
            {
                children.AddRange(composite.m_children);
            }

            children.RemoveAll(c => c == null);
            return children;
        }

        public void Traverse(Node node, System.Action<Node> callback)
        {
            if (node != null)
            {
                callback(node);
                List<Node> children = GetChildren(node);
                children.ForEach(n => Traverse(n, callback));
            }
        }

        public BehaviourTree Clone()
        {
            BehaviourTree clone = Instantiate(this);
            clone.name = clone.name.Replace("(Clone)", " (Runtime)");

            // clone nodes
            clone.m_root = m_root.Clone();
            clone.m_nodes = new List<Node>();
            Traverse(clone.m_root, (n) => { clone.m_nodes.Add(n); });

            return clone;
        }
    }
}