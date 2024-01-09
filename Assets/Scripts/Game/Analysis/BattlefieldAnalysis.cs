using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Analysis
{
    [RequireComponent(typeof(Canvas))]
    public abstract class BattlefieldAnalysis : MonoBehaviour
    {
        Dictionary<Battlefield.Node, Text> m_nodeTexts = new Dictionary<Battlefield.Node, Text>();

        #region Properties

        protected abstract float UpdateInterval { get; }

        #endregion

        private void Start()
        {
            StartCoroutine(Logic());    
        }

        IEnumerator Logic()
        {
            while (Battlefield.Instance == null)
            {
                yield return null;
            }
            yield return null;

            // create node texts
            GameObject nodeTemplate = transform.Find("NodeTemplate").gameObject;
            if (nodeTemplate != null) 
            {
                foreach(Battlefield.Node node in Battlefield.Instance.Nodes) 
                {
                    GameObject go = Instantiate(nodeTemplate, transform);
                    go.transform.position = node.WorldPosition + Vector3.up * 0.01f;
                    Text txt = go.GetComponent<Text>();
                    txt.text = "";
                    go.SetActive(true);
                    m_nodeTexts.Add(node, txt);
                }
            }

            do
            {
                // update node scores
                foreach (Battlefield.Node node in Battlefield.Instance.Nodes)
                {
                    bool bShow;
                    m_nodeTexts[node].text = CalculateScore(node, out bShow).ToString("0.0");
                    m_nodeTexts[node].gameObject.SetActive(bShow);
                }

                if (UpdateInterval > 0)
                {
                    yield return new WaitForSeconds(UpdateInterval);
                }
            }
            while (UpdateInterval > 0.0f);
        }

        protected abstract float CalculateScore(Battlefield.Node node, out bool bShow);
    }
}