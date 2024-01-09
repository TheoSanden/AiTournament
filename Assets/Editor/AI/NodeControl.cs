using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using AI.Nodes;

namespace AI
{
    public class NodeControl : UnityEditor.Experimental.GraphView.Node
    {
        private Nodes.Node  m_node;
        private Port        m_input;
        private Port        m_output;
        private Label       m_description;

        #region Properties

        public Nodes.Node Node => m_node;

        public Port Input => m_input;

        public Port Output => m_output;

        #endregion

        public NodeControl(Nodes.Node node) : base("Assets/Editor/AI/NodeControl.uxml")
        {
            m_node = node;
            title = node.name;
            viewDataKey = node.ID.ToString();

            style.left = node.m_vPosition.x;
            style.top = node.m_vPosition.y;

            CreatePorts();

            if (m_node is ActionNode)
            {
                AddToClassList("action");
            }
            else if (m_node is CompositeNode)
            {
                AddToClassList("composite");
            }
            else if (m_node is Root)
            {
                AddToClassList("root");
            }
            else if (m_node is DecoratorNode)
            {
                AddToClassList("decorator");
            }

            m_description = this.Q<Label>("description");
            m_description.text = m_node.Description;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Undo.RecordObject(Node, "Moving State");
            m_node.m_vPosition = new Vector2(newPos.xMin, newPos.yMin);
            EditorUtility.SetDirty(Node);
        }

        private void CreatePorts()
        {
            // create input port
            if (m_node is not Root)
            {
                m_input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
                m_input.portName = "";
                m_input.style.flexDirection = FlexDirection.Column;
                //m_input.style.alignSelf = Align.FlexStart;
                inputContainer.Add(m_input);
            }

            // create output port
            if(m_node is CompositeNode)
            {
                m_output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            }
            else if (m_node is DecoratorNode ||
                     m_node is Root)
            {
                m_output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            }

            if (m_output != null)
            {
                m_output.portName = "";
                m_output.style.flexDirection = FlexDirection.ColumnReverse;
                //m_output.style.alignSelf = Align.FlexEnd;
                outputContainer.Add(m_output);
            }
        }

        public override void OnSelected()
        {
            base.OnSelected();
            Selection.activeObject = Node;
        }

        public void UpdateState()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            RemoveFromClassList("running");
            RemoveFromClassList("success");
            RemoveFromClassList("failure");
            m_description.text = m_node.Description;

            switch (m_node.m_state)
            {
                case Nodes.Node.State.Running:
                    if (m_node.m_bStarted)
                    {
                        AddToClassList("running");
                    }
                    break;
                case Nodes.Node.State.Success:
                    AddToClassList("success");
                    break;
                case Nodes.Node.State.Failure:
                    AddToClassList("failure");
                    break;
            }
        }
    }
}