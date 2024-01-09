using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System;
using System.Linq;
using AI.Nodes;

namespace AI
{
    public class BehaviourTreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, GraphView.UxmlTraits> { }

        private BehaviourTree      m_tree;

        #region Properties

        #endregion

        public BehaviourTreeView()
        {
            Insert(0, new GridBackground());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/AI/BehaviourTreeEditor.uss");
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            PopulateView(m_tree);
            AssetDatabase.SaveAssets();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //base.BuildContextualMenu(evt);

            // create states
            var types = TypeCache.GetTypesDerivedFrom<Nodes.Node>();
            foreach (var type in types)
            {
                if (!type.IsAbstract && type != typeof(Root))
                {
                    evt.menu.AppendAction("Create Node/" + type.Name, (a) => CreateNode(type));
                }
            }
        }
       
        private void CreateNode(System.Type type)
        {
            if (m_tree != null)
            {
                Nodes.Node node = m_tree.CreateNode(type);
                CreateNodeControl(node);
            }
        }

        internal void PopulateView(BehaviourTree tree)
        {
            m_tree = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            if (m_tree != null)
            {
                // create root node?
                if (m_tree.m_root == null) 
                {
                    m_tree.m_root = m_tree.CreateNode(typeof(Root)) as Root;
                    EditorUtility.SetDirty(m_tree);
                    AssetDatabase.SaveAssetIfDirty(m_tree);
                }

                // create nodes
                m_tree.m_nodes.ForEach(n => CreateNodeControl(n));

                // create edges
                m_tree.m_nodes.ForEach(n =>
                {
                    List<Nodes.Node> children = m_tree.GetChildren(n);
                    NodeControl parentControl = FindNodeControl(n);
                    children.ForEach(c =>
                    {
                        NodeControl childControl = FindNodeControl(c);
                        Edge edge = parentControl.Output.ConnectTo(childControl.Input);
                        AddElement(edge);
                    });
                });
            }
        }

        void CreateNodeControl(Nodes.Node node)
        {
            if (node != null)
            {
                NodeControl snc = new NodeControl(node);
                AddElement(snc);
            }
        }

        NodeControl FindNodeControl(Nodes.Node node)
        {
            return GetNodeByGuid(node.ID.ToString()) as NodeControl;
        }

        public void UpdateNodeStates()
        {
            nodes.ForEach(n => 
            {
                if (n is NodeControl snc)
                {
                    snc.UpdateState();
                }
            });
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            // remove elements
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(e =>
                {
                    if (e is NodeControl snc)
                    {
                        m_tree.DeleteNode(snc.Node);
                    }

                    if (e is Edge edge)
                    {
                        NodeControl from = edge.output.node as NodeControl;
                        NodeControl to = edge.input.node as NodeControl;
                        m_tree.RemoveChild(from.Node, to.Node);
                    }
                });
            }

            // create edges
            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge => 
                {
                    NodeControl from = edge.output.node as NodeControl;
                    NodeControl to = edge.input.node as NodeControl;
                    m_tree.AddChild(from.Node, to.Node);
                });
            }

            // moved?
            if (graphViewChange.movedElements != null && m_tree != null)
            {
                foreach (Nodes.Node node in m_tree.m_nodes)
                {
                    if (node is CompositeNode compositeNode)
                    {
                        compositeNode.SortChildren();
                    }
                }
            }

            return graphViewChange;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort => 
                endPort.direction != startPort.direction && 
                endPort.node != startPort.node).ToList();
        }
    }
}