using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AI
{
    [CustomEditor(typeof(Brain), true)]
    public class BrainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // debug blackboard
            Brain brain = target as Brain;
            if (brain.Tree != null && 
                Application.isPlaying &&
                brain.Tree.Blackboard != null)
            {
                Blackboard bb = brain.Tree.Blackboard;
                GUILayout.Space(10);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Blackboard (" + bb.Count + ")", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                // draw each blackboard item
                foreach (KeyValuePair<string, object> kvp in bb.Items)
                {
                    EditorGUILayout.LabelField(kvp.Key, kvp.Value.ToString());                
                }

                EditorGUI.indentLevel--;
                GUILayout.EndVertical();

                Repaint();
            }
        }

    }
}