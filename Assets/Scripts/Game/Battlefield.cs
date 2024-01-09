using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public partial class Battlefield : MonoBehaviour, ISearchableGraph
    {
        public abstract class Node : IPositionNode
        {
            public Link             m_parentLink;
            public float            m_fDistance;
            public float            m_fRemainingDistance;

            protected Vector2Int    m_vPosition;
            protected List<Link>    m_links = new List<Link>();

            #region Properties

            public Vector2Int Position => m_vPosition;

            public Vector3 WorldPosition => new Vector3(m_vPosition.x, 0.0f, m_vPosition.y);

            public IEnumerable<ILink> Links => m_links;

            public virtual float AdditionalCost => 0.0f;

            public abstract float UnitMoveSpeed { get; }

            public abstract Color Color { get; }

            public Unit Unit { get; set; }

            #endregion

            public Node(Vector2Int vPosition)
            {
                m_vPosition = vPosition;
            }

            public void AddLink(Link link)
            {
                if (link != null && !m_links.Contains(link))
                {
                    m_links.Add(link);
                }
            }

            public abstract void CreateLinks();

            public void ResetPathfinding()
            {
                m_fDistance = float.MaxValue;
                m_fRemainingDistance = float.MaxValue;
                m_parentLink = null;
            }

            public virtual void OnEnter()
            {
            }
        }

        public abstract class Link : Graphs.Link
        {
            #region Properties

            public new Node Source => base.Source as Node;

            public new Node Target => base.Target as Node;

            public virtual float AdditionalCost => 0.0f;

            #endregion

            public Link(Node source, Node target) : base(source, target)
            {
            }

            public abstract IEnumerator MoveUnit(Unit unit);
        }

        [SerializeField]
        private GameObject                      m_rockPrefab;

        private Node[,]                         m_nodes;
        private Camera                          m_camera;

        private static Battlefield              sm_instance;
        private static readonly Vector2Int[]    sm_coverDirections = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        public static readonly Vector2Int       SIZE = new Vector2Int(48, 32);

        #region Properties

        public IEnumerable<INode> Nodes
        {
            get
            {
                if (m_nodes != null)
                {
                    for (int y = 0; y < SIZE.y; ++y)
                    {
                        for (int x = 0; x < SIZE.x; ++x)
                        {
                            if (m_nodes[x, y] is not Node_Rock)
                            {
                                yield return m_nodes[x, y];
                            }
                        }
                    }
                }
            }
        }

        public Node this[int x, int y]
        {
            get
            {
                if (m_nodes != null &&
                    x >= 0 && x < SIZE.x &&
                    y >= 0 && y < SIZE.y)
                {
                    return m_nodes[x, y];
                }

                return null;
            }
        }

        public Node this[Vector2Int c] => this[c.x, c.y];

        public Camera Camera => m_camera;

        public static Battlefield Instance => sm_instance;

        #endregion

        protected virtual void OnEnable()
        {
            sm_instance = this;
            m_camera = GetComponentInChildren<Camera>();
        }

        public void CreateNewLevel()
        {
            // setup ground collider
            BoxCollider bc = GetComponent<BoxCollider>();
            bc.size = new Vector3(SIZE.x, 0.1f, SIZE.y);
            bc.center = new Vector3(SIZE.x * 0.5f - 0.5f, -0.05f, SIZE.y * 0.5f - 0.5f);

            // remove old children
            while (transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
                child.parent = null;
                Destroy(child.gameObject);
            }

            // initialize
            CreateNodes();
            CreateMesh();
        }

        protected void Update()
        {
            // speedup?
            if (Input.GetKeyDown(KeyCode.X))
            {
                Time.timeScale = 5.0f;
            }

            if (Input.GetKeyUp(KeyCode.X))
            {
                Time.timeScale = 1.0f;
            }
        }

        protected void CreateNodes()
        {
            // create mud & rock tile symmetry
            Dictionary<Vector2Int, bool> mudTiles = new Dictionary<Vector2Int, bool>();
            Dictionary<Vector2Int, bool> rockTiles = new Dictionary<Vector2Int, bool>();
            for (int i = 0; i < 256; ++i)
            {
                Dictionary<Vector2Int, bool> target = i < 192 ? mudTiles : rockTiles;
                Vector2Int c = new Vector2Int(Random.Range(3, SIZE.x - 3), Random.Range(0, SIZE.y));
                target[c] = true;
                target[new Vector2Int(SIZE.x - 1 - c.x, SIZE.y - 1 - c.y)] = true;
            }

            // create nodes
            m_nodes = new Node[SIZE.x, SIZE.y];
            for (int y = 0; y < SIZE.y; ++y)
            {
                for (int x = 0; x < SIZE.x; ++x)
                {
                    Vector2Int c = new Vector2Int(x, y);
                    m_nodes[x, y] = mudTiles.ContainsKey(c) ? new Node_Mud(c) : 
                                    rockTiles.ContainsKey(c) ? new Node_Rock(c) :
                                                                new Node_Grass(c);
                }
            }

            // create links
            for (int y = 0; y < SIZE.y; ++y)
            {
                for (int x = 0; x < SIZE.x; ++x)
                {
                    m_nodes[x, y].CreateLinks();
                }
            }
        }

        protected void CreateMesh()
        {
            // calculate mesh data
            Vector3[] corners = new Vector3[]{
                new Vector3(-0.4f, 0.0f, -0.4f),
                new Vector3(0.4f, 0.0f, -0.4f),
                new Vector3(0.4f, 0.0f, 0.4f),
                new Vector3(-0.4f, 0.0f, 0.4f),
            };

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Color> colors = new List<Color>();
            List<int> triangles = new List<int>();
            Dictionary<Node, int> nodeStarts = new Dictionary<Node, int>();

            for (int y = 0; y < SIZE.y; ++y)
            {
                for (int x = 0; x < SIZE.x; ++x)
                {
                    Node node = this[x, y];
                    int iStart = vertices.Count;
                    nodeStarts[node] = iStart;
                    vertices.AddRange(System.Array.ConvertAll(corners, c => c + node.WorldPosition));
                    uvs.AddRange(System.Array.ConvertAll(corners, c => new Vector2(c.x + node.WorldPosition.x, c.z + node.WorldPosition.z) * 0.25f));
                    colors.AddRange(System.Array.ConvertAll(corners, c => node.Color));
                    triangles.AddRange(new int[] { iStart + 0, iStart + 3, iStart + 1, iStart + 1, iStart + 3, iStart + 2 });
                }
            }

            // calculate border triangles
            for (int y = 0; y < SIZE.y; ++y)
            {
                for (int x = 0; x < SIZE.x; ++x)
                {
                    Node n1 = this[x, y];
                    Node n2 = this[x + 1, y];
                    Node n3 = this[x + 1, y + 1];
                    Node n4 = this[x, y + 1];

                    int iStart1, iStart2 = 0, iStart3 = 0, iStart4 = 0;
                    nodeStarts.TryGetValue(n1, out iStart1);

                    if (n2 != null && nodeStarts.TryGetValue(n2, out iStart2))
                    {
                        triangles.AddRange(new int[] { iStart2 + 0, iStart1 + 1, iStart1 + 2, iStart2 + 0, iStart1 + 2, iStart2 + 3 });
                    }

                    if (n4 != null && nodeStarts.TryGetValue(n4, out iStart4))
                    {
                        triangles.AddRange(new int[] { iStart4 + 1, iStart1 + 2, iStart1 + 3, iStart4 + 1, iStart1 + 3, iStart4 + 0 });
                    }

                    if (n3 != null && nodeStarts.TryGetValue(n3, out iStart3))
                    {
                        triangles.AddRange(new int[] { iStart2 + 3, iStart1 + 2, iStart4 + 1, iStart2 + 3, iStart4 + 1, iStart3 + 0 });
                    }
                }
            }

            // create mesh
            Mesh mesh = new Mesh();
            mesh.name = "BattlefieldMesh";
            mesh.hideFlags = HideFlags.DontSave;
            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.colors = colors.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // assign mesh
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf.mesh != null)
            {
                Destroy(mf.mesh);
            }
            mf.mesh = mesh;

            // spawn rock meshes
            for (int y = 0; y < SIZE.y; ++y)
            {
                for (int x = 0; x < SIZE.x; ++x)
                {
                    Node node = this[x, y];
                    if (node is Node_Rock)
                    {
                        GameObject go = Instantiate(m_rockPrefab, transform);
                        go.name = "Rock";
                        go.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                        go.transform.position = node.WorldPosition;
                        go.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0, 4) * 90.0f, 0.0f);
                    }
                }
            }
        }

        public virtual Node GetRandomNode()
        {
            List<INode> nodes = new List<INode>(Nodes);
            return nodes[Random.Range(0, nodes.Count)] as Node;
        }

        public bool HasAnyCoverAt(Node node)
        {
            foreach (Vector2Int coverDir in sm_coverDirections)
            {
                Vector2Int coverPos = node.Position + coverDir;
                Node coverNode = this[coverPos];
                if (coverNode is Node_Rock)
                {
                    return true;
                }
            }

            return false;
        }

        public bool InCover(Node node, Node damageSource)
        {
            return node != null && damageSource != null && InCover(node, damageSource.WorldPosition);
        }

        public bool InCover(Node node, Vector3 vDamageSource)
        {
            if (node != null)
            {
                foreach (Vector2Int coverDir in sm_coverDirections)
                {
                    Vector2Int coverPos = node.Position + coverDir;
                    Node coverNode = this[coverPos];
                    if (coverNode is Node_Rock)
                    {
                        // 120 degree protection from cover
                        Vector3 vToCover = Vector3.Normalize(coverNode.WorldPosition - node.WorldPosition);
                        Vector3 vToDamage = Vector3.Normalize(vDamageSource - node.WorldPosition);
                        if (Vector3.Angle(vToCover, vToDamage) <= 120.0f)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public float Heuristic(INode start, INode goal)
        {
            if (start is IPositionNode startNode &&
                goal is IPositionNode goalNode)
            {
                return Vector3.Distance(startNode.WorldPosition, goalNode.WorldPosition);
            }

            return 1.0f;
        }
    }
}