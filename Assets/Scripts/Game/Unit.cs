using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public abstract class Unit : MonoBehaviour
    {
        private Battlefield.Node    m_node;
        private Team                m_team;
        private Transform           m_health;
        private float               m_fHealth = MAX_HP;
        private GameObject[]        m_healthGUI;
        private float               m_fFireCooldown;
        private GameObject          m_muzzleFlame;
        private LineRenderer        m_bullet;
        private Battlefield.Link    m_currentLink;
        private GameObject          m_shield;

        static GameObject           sm_bloodPrefab;

        public const int            MAX_HP = 3;
        public const float          FIRE_RANGE = 10.0f;
        public const float          FIRE_COOLDOWN_TIME = 0.5f;
        public const float          DAMAGE = 0.1f;
        public const float          COVER_REDUCTION = 0.5f;

        #region Properties

        public Battlefield.Node CurrentNode
        {
            get => m_node;
            protected set
            {
                if (m_node != null) m_node.Unit = null;
                m_node = value;
                if (m_node != null) m_node.Unit = this;
            }
        }

        public Team Team => m_team;

        public int Health => Mathf.RoundToInt(m_fHealth);

        public Battlefield.Node TargetNode { get; set; }

        public IEnumerable<Unit> Friendlies => Team.GetComponentsInChildren<Unit>();

        public IEnumerable<Unit> Enemies => Team.EnemyTeam.GetComponentsInChildren<Unit>();

        public IEnumerable<Unit> EnemiesInRange
        {
            get
            {
                foreach (Unit enemy in Enemies)
                {
                    if (Vector3.Distance(enemy.transform.position, transform.position) < FIRE_RANGE)
                    {
                        yield return enemy;
                    }
                }
            }
        }

        public Unit ClosestEnemy
        {
            get
            {
                float fBestDistance = float.MaxValue;
                Unit closestEnemy = null;
                foreach (Unit enemy in Enemies)
                {
                    float fDistance = Vector3.Distance(enemy.transform.position, transform.position);
                    if (fDistance < fBestDistance)
                    {
                        fBestDistance = fDistance;
                        closestEnemy = enemy; 
                    }
                }

                return closestEnemy;
            }
        }

        public bool IsMoving => m_currentLink != null;

        public bool CanShoot => m_currentLink == null || (m_currentLink.Source is not Node_Mud && m_currentLink.Target is not Node_Mud);

        public bool InCover => !IsMoving && Battlefield.Instance.HasAnyCoverAt(CurrentNode);

        #endregion

        protected virtual void OnEnable()
        {
            m_team = GetComponentInParent<Team>();
            m_muzzleFlame = transform.Find("MuzzleFlame").gameObject;
            m_bullet = transform.Find("Bullet").GetComponent<LineRenderer>();
            m_shield = transform.Find("Health/Shield").gameObject;

            if (sm_bloodPrefab == null)
            {
                sm_bloodPrefab = Resources.Load<GameObject>("Prefabs/Blood");
            }
        }

        protected virtual void Start()
        {
            // find start node
            CurrentNode = GraphUtils.GetClosestNode<Battlefield.Node>(Battlefield.Instance, transform.position);
            transform.position = CurrentNode.WorldPosition;

            // initialize health GUI
            m_health = transform.Find("Health");
            GameObject hpTemplate = m_health.Find("HP").gameObject;
            m_healthGUI = new GameObject[MAX_HP];
            for (int i = 0; i < MAX_HP; ++i)
            {
                GameObject hp = Instantiate(hpTemplate, hpTemplate.transform.parent);
                hp.name = hpTemplate.name;
                Image img = hp.GetComponentInChildren<Image>();
                img.color = Team.Color;
                img.GetComponent<Outline>().effectColor = Team.Color * 0.5f;
                m_healthGUI[i] = hp;
            }
            Destroy(hpTemplate);

            // start moving
            StartCoroutine(MoveLogic());
        }

        public void TakeDamage(Vector3 vSource)
        {
            // Cover reduction?
            float fDamage = DAMAGE * (!IsMoving && Battlefield.Instance.InCover(CurrentNode, vSource) ? COVER_REDUCTION : 1.0f);

            // take some damage
            m_fHealth -= fDamage;
            SpawnBlood();
            UpdateHealthGUI();

            // death?
            if (m_fHealth <= 0.0f)
            {
                // spawn lots of blood
                for (int i = 0; i < 3; ++i)
                {
                    SpawnBlood();
                }

                // free up node
                CurrentNode = null;

                // self destruct!
                Destroy(gameObject);
            }
        }

        private void UpdateHealthGUI()
        {            
            for (int i = 0; i < MAX_HP; ++i)
            {
                m_healthGUI[i].SetActive(i < Mathf.RoundToInt(m_fHealth));
            }
        }

        protected virtual void Update()
        {
            // disable "VFX"
            m_muzzleFlame.SetActive(false);
            m_bullet.gameObject.SetActive(false);

            // move and rotate health to face camera
            if (m_health != null)
            {
                m_health.position = transform.position + Vector3.forward * 0.5f + Vector3.up * 0.1f;
                m_health.rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
            }

            // in cover?
            m_shield.SetActive(InCover);

            // fire?
            m_fFireCooldown -= Time.deltaTime;
            if (m_fFireCooldown < 0.0f && CanShoot)
            {
                List<Unit> enemiesInRange = new List<Unit>(EnemiesInRange);
                if (enemiesInRange.Count > 0)
                {
                    Unit target = SelectTarget(enemiesInRange);
                    if (target != null)
                    {
                        // face enemy
                        Vector3 vToEnemy = target.transform.position - transform.position;
                        vToEnemy.y = 0.0f;
                        if (vToEnemy.magnitude > 0.001f)
                        {
                            transform.rotation = Quaternion.LookRotation(vToEnemy.normalized);
                        }

                        // enemy takes damage
                        target.TakeDamage(transform.position);

                        // reset cooldown
                        m_fFireCooldown = FIRE_COOLDOWN_TIME;

                        // Special Effects ;)
                        m_muzzleFlame.SetActive(true);
                        m_bullet.SetPosition(0, m_muzzleFlame.transform.position);
                        m_bullet.SetPosition(1, target.transform.position + Vector3.up);
                        m_bullet.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void SpawnBlood()
        {
            // spawn some blood
            if (sm_bloodPrefab != null)
            {
                GameObject go = Instantiate(sm_bloodPrefab);
                go.name = sm_bloodPrefab.name;
                go.hideFlags = HideFlags.HideInHierarchy;
                go.transform.position = transform.position + Vector3.up * Random.Range(0.5f, 1.5f);
            }
        }

        protected abstract Unit SelectTarget(List<Unit> enemiesInRange);

        protected virtual GraphUtils.Path GetPathToTarget()
        {
            return Team.GetShortestPath(CurrentNode, TargetNode);
        }

        private IEnumerator MoveLogic()
        {
            while (true)
            {
                if (CurrentNode != null &&
                   TargetNode != null &&
                   TargetNode != CurrentNode)
                {
                    // get path
                    GraphUtils.Path path = GetPathToTarget();
                    if(path != null && 
                       path.Count > 0 &&
                       path[0] is Battlefield.Link link) 
                    {
                        // move along first link
                        CurrentNode = link.Target;
                        m_currentLink = link;
                        yield return StartCoroutine(link.MoveUnit(this));
                        m_currentLink = null;
                    }
                    else
                    {
                        // no path to goal? sleep!
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                else
                {
                    // no target node? move towards current node center
                    if (CurrentNode != null)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, CurrentNode.WorldPosition, Time.deltaTime);
                    }

                    yield return null;
                }
            }
        }
    }
}