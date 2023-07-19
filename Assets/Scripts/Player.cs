using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{
    [SerializeField] bool isPlayer;
    [SerializeField] protected int oponentLayerIndex;
    [SerializeField] Color color;
    [SerializeField] Material material;
    [SerializeField] string characterName;
    [SerializeField] protected GameManager gameManagerRef;

    [HideInInspector]
    protected List<Player> attackedCharacters = new List<Player>();

    
    

    [Header("Area")]
    [SerializeField] int initialAreaPoints = 45;
    [SerializeField] float initialAreaRadius = 3f;
    [SerializeField] float minPointDistance = 0.1f;

    [HideInInspector]
    public GameObject areaOutline;
    [HideInInspector]
    public List<Vector3> CurrentCoveredAreaVertices = new List<Vector3>();
    [HideInInspector]
    public List<Vector3> newCoveredAreaVertices = new List<Vector3>();

    [HideInInspector]
    public PlayerArea area;

    // private fields
    private MeshRenderer areaMeshRend;
    private MeshFilter areaFilter;
    private MeshRenderer areaOutlineMeshRend;
    private MeshFilter areaOutlineFilter;

    [Space]
    [Header("Movement")]
    public float speed = 2f;
    public float turnSpeed = 14f;


    [HideInInspector]
    public TrailRenderer trail;
    [HideInInspector]
    public GameObject trailCollidersHolder;
    [HideInInspector]
    public List<SphereCollider> trailColls = new List<SphereCollider>();

    protected Rigidbody rb;
    protected Vector3 curDir;
    protected Quaternion targetRot;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = transform.Find("Trail").GetComponent<TrailRenderer>();
        trail.material.color = new Color(color.r, color.g, color.b, 0.65f);
        GetComponent<MeshRenderer>().material.color = new Color(color.r * 1.3f, color.g * 1.3f, color.b * 1.3f);
    }
    public virtual void Start()
    {
        InitializeCharacter();
    }

    public virtual void Update()
    {

        transform.position += transform.forward * speed * Time.deltaTime;

        float dir = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * dir * turnSpeed * Time.deltaTime);

        PlayerLogicPaperMethod();
    }
    private void InitializeCharacter()
    {
        area = new GameObject().AddComponent<PlayerArea>();
        area.name = characterName + "Area";
        area.player = this;
        Transform areaTrans = area.transform;
        areaFilter = area.gameObject.AddComponent<MeshFilter>();
        areaMeshRend = area.gameObject.AddComponent<MeshRenderer>();
        areaMeshRend.material = material;
        areaMeshRend.material.color = color;

        areaOutline = new GameObject();
        areaOutline.name = characterName + "AreaOutline";
        Transform areaOutlineTrans = areaOutline.transform;
        areaOutlineTrans.position += new Vector3(0, -0.495f, -0.1f);
        areaOutlineTrans.SetParent(areaTrans);
        areaOutlineFilter = areaOutline.AddComponent<MeshFilter>();
        areaOutlineMeshRend = areaOutline.AddComponent<MeshRenderer>();
        areaOutlineMeshRend.material = material;
        areaOutlineMeshRend.material.color = new Color(color.r * .7f, color.g * .7f, color.b * .7f);

        float step = 360f / initialAreaPoints;
        for (int i = 0; i < initialAreaPoints; i++)
        {
            CurrentCoveredAreaVertices.Add(transform.position + Quaternion.Euler(new Vector3(0, step * i, 0)) * Vector3.forward * initialAreaRadius);
        }
        UpdatePlayerArea();

        trailCollidersHolder = new GameObject();
        trailCollidersHolder.transform.SetParent(areaTrans);
        trailCollidersHolder.name = characterName + "TrailCollidersHolder";
        trailCollidersHolder.layer = 8;
    }
    public void UpdatePlayerArea()
    {
        if (areaFilter)
        {
            Mesh areaMesh = CreateMesh(characterName, CurrentCoveredAreaVertices);
            areaFilter.mesh = areaMesh;
            areaOutlineFilter.mesh = areaMesh;
            area.playerMeshCollider.sharedMesh = areaMesh;
        }
    }

    private Mesh CreateMesh(string meshName, List<Vector3> vertices)
    {
        Triangulator tr = new Triangulator(Vertices2D(vertices));
        int[] indices = tr.Triangulate();

        Mesh msh = new Mesh();
        msh.vertices = vertices.ToArray();
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();
        msh.name = meshName + "Mesh";

        return msh;
    }
    private Vector2[] Vertices2D(List<Vector3> vertices)
    {
        List<Vector2> areaVertices2D = new List<Vector2>();
        foreach (Vector3 vertex in vertices)
        {
            areaVertices2D.Add(new Vector2(vertex.x, vertex.z));
        }



        return areaVertices2D.ToArray();
    }
    public int GetNearbyAreaVerticesPoints(Vector3 fromPos)
    {
        int closest = -1;
        float closestDist = Mathf.Infinity;
        for (int i = 0; i < CurrentCoveredAreaVertices.Count; i++)
        {
            float dist = (CurrentCoveredAreaVertices[i] - fromPos).magnitude;
            if (dist < closestDist)
            {
                closest = i;
                closestDist = dist;
            }
        }

        return closest;
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        PlayerArea characterArea = other.GetComponent<PlayerArea>();

        
            if (characterArea && characterArea != area && !attackedCharacters.Contains(characterArea.player))
            {
                attackedCharacters.Add(characterArea.player);
            }

            if (other.gameObject.layer == 8)
            {
            Debug.Log("This player thing is problem " +other.name);
                characterArea = other.transform.parent.GetComponent<PlayerArea>();
                characterArea.player.Die();
            }
                
    }
    public void PlayerLogicPaperMethod()
    {
        var trans = transform;
        var transPos = trans.position;
        trans.position = Vector3.ClampMagnitude(transPos, 24.5f);
        bool isOutside = !GameManager.IsPointInPolygon(new Vector2(transPos.x, transPos.z), Vertices2D(CurrentCoveredAreaVertices));
        int count = newCoveredAreaVertices.Count;

        if (isOutside)
        {
            if (count == 0 || !newCoveredAreaVertices.Contains(transPos) && (newCoveredAreaVertices[count - 1] - transPos).magnitude >= minPointDistance)
            {
                count++;
                newCoveredAreaVertices.Add(transPos);

                int trailCollsCount = trailColls.Count;
                float trailWidth = trail.startWidth;
                SphereCollider lastColl = trailCollsCount > 0 ? trailColls[trailCollsCount - 1] : null;
                if(!lastColl || (transPos - lastColl.center).magnitude > trailWidth)
                {
                    SphereCollider trailCollider = trailCollidersHolder.AddComponent<SphereCollider>();
                    trailCollider.center = transPos;
                    trailCollider.radius = trailWidth / 2f;
                    trailCollider.isTrigger = true;
                    trailCollider.enabled = false;
                    trailColls.Add(trailCollider);

                    if (trailCollsCount > 1)
                    {
                        trailColls[trailCollsCount - 2].enabled = true;
                    }
                }
            }

            if (!trail.emitting)
            {
                trail.Clear();
                trail.emitting = true;
            }
        }
        else if (count > 0)
        {
            GameManager.DeformCharacterArea(this, newCoveredAreaVertices);

            foreach (var character in attackedCharacters)
            {
                List<Vector3> newCharacterAreaVertices = new List<Vector3>();
                foreach (var vertex in newCoveredAreaVertices)
                {
                    if (GameManager.IsPointInPolygon(new Vector2(vertex.x, vertex.z), Vertices2D(character.CurrentCoveredAreaVertices)))
                    {
                        newCharacterAreaVertices.Add(vertex);
                    }
                }

                GameManager.DeformCharacterArea(character, newCharacterAreaVertices);
            }
            attackedCharacters.Clear();
            newCoveredAreaVertices.Clear();

            if (trail.emitting)
            {
                trail.Clear();
                trail.emitting = false;
            }
            foreach (var trailColl in trailColls)
            {
                Destroy(trailColl);
            }
            trailColls.Clear();
        }
    }
    public void Die()
    {
        if(isPlayer)
        {
            Debug.Log("Dead..");
            gameManagerRef.GameOver("YOU DEAD!");
        }
        else
        {
            Destroy(area.gameObject);
            Destroy(areaOutline);
            Destroy(gameObject);
        }

           
        
    }
}
