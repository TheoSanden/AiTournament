using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class BallsOfSteel : MonoBehaviour
    {
        [SerializeField]
        public GameObject       m_team1;

        [SerializeField]
        public GameObject       m_team2;


        private void Start()
        {
            StartCoroutine(GameLogic());     
        }

        IEnumerator GameLogic()
        {
            // wait for battlefield
            while (Battlefield.Instance == null)
            {
                yield return null;
            }

            // game loop
            while (true)
            {
                // setup battlefield
                Battlefield.Instance.CreateNewLevel();
                yield return null;

                // create teams
                Team[] teams = CreateTeams();
                if (teams == null)
                {
                    break;
                }

                // let Teams & Units execute their Start() functions
                yield return null;

                // start banner
                yield return StartCoroutine(ShowBanner("<color=#" + ColorUtility.ToHtmlStringRGBA(teams[0].Color) + ">" + teams[0].name + "</color> vs " +
                                                       "<color=#" + ColorUtility.ToHtmlStringRGBA(teams[1].Color) + ">" + teams[1].name + "</color>"));

                // wait for winner
                Team winner = null;
                while (true)
                {
                    int iLoser = System.Array.FindIndex(teams, t => t.UnitCount == 0);
                    if (iLoser >= 0)
                    {
                        winner = teams[(iLoser + 1) % teams.Length];
                        break;
                    }

                    yield return new WaitForSeconds(0.1f);
                }

                // announce winner
                yield return StartCoroutine(ShowBanner("Winner is <color=#" + ColorUtility.ToHtmlStringRGBA(winner.Color) + ">" + winner.name + "</color>"));
                yield return new WaitForSeconds(1.0f);
            }
        }

        private Team[] CreateTeams()
        {
            const int NUM_UNITS = 5;

            if (m_team1 == null ||
                m_team2 == null)
            {
                Debug.LogError("Needs 2 teams to start 'Balls of Steel'.");
                return null;
            }

            // create teams
            Team[] teams = new Team[2];
            for (int i = 0; i < 2; ++i)
            {
                GameObject prefab = i == 0 ? m_team1 : m_team2;
                GameObject goTeam = Instantiate(prefab, Battlefield.Instance.transform);
                goTeam.name = prefab.name;
                teams[i] = goTeam.GetComponent<Team>();

                // find units
                List<Unit> units = new List<Unit>(goTeam.GetComponentsInChildren<Unit>());
                if (units.Count != NUM_UNITS)
                {
                    Debug.LogError("Team: " + goTeam.name + " does not contain " + NUM_UNITS + " units.");
                    return null;
                }

                // place units
                for (int j = 0; j < NUM_UNITS; ++j)
                {
                    Vector2Int c = new Vector2Int(i == 1 ? 1 : Battlefield.SIZE.x - 2, Battlefield.SIZE.y / 2 - (NUM_UNITS / 2) * 2 + j * 2 - i);
                    Battlefield.Node node = Battlefield.Instance[c.x, c.y];
                    units[j].transform.position = node.WorldPosition;
                    units[j].transform.rotation = i == 0 ? Quaternion.identity : Quaternion.Euler(0.0f, 180.0f, 0.0f);
                    MeshRenderer mr = units[j].GetComponent<MeshRenderer>();
                    Material material = mr.material;
                    material.color = teams[i].Color;
                }
            }

            return teams;
        }

        IEnumerator ShowBanner(string message)
        {
            // set message
            CanvasGroup cg = transform.Find("Canvas").GetComponent<CanvasGroup>();
            Text txt = cg.transform.Find("Banner/Text").GetComponent<Text>();
            txt.text = message;
            cg.gameObject.SetActive(true);

            // fade in
            for (float f = 0.0f; f < 1.0f; f += Time.deltaTime)
            {
                cg.alpha = f;
                yield return null;
            }

            // wait
            cg.alpha = 1.0f;
            yield return new WaitForSeconds(2.0f);

            // fade out
            for (float f = 0.0f; f < 1.0f; f += Time.deltaTime)
            {
                cg.alpha = 1.0f - f;
                yield return null;
            }

            // done
            cg.alpha = 0.0f;
            cg.gameObject.SetActive(false);
        }
    }
}