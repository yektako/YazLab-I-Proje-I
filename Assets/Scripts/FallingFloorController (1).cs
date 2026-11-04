using Unity.AI.Navigation;
using UnityEngine;

public class FallingFloorController : MonoBehaviour
{
    [SerializeField] private NavmeshController navmeshController;
    [Tooltip("Platform hangi Y koordinatına inecek")]
    [SerializeField] private float finalYPosition = 0;

    [Tooltip("Platform kaç saniyede düşecek")]
    [SerializeField] private float descentPeriod = 1f;
    [SerializeField] private Material triggeredMaterial;
    [SerializeField] private Material descendingMaterial;
    private MeshRenderer renderer;
    private bool isDescending = false;
    void Start()
    {
        renderer = gameObject.GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (isDescending)
        {
            if (transform.position.y > finalYPosition)
            {
                transform.position -= descentPeriod * Time.deltaTime * Vector3.up;
                if (transform.position.y < finalYPosition)
                transform.position = new Vector3(transform.position.x, finalYPosition, transform.position.z);
            }
        }
    }
    public void DetectorTriggerEnter(Collider other)
    {
        if (!isDescending) renderer.material = triggeredMaterial;
    }
    public void DetectorTriggerExit(Collider other)
    {
        isDescending = true;
        renderer.material = descendingMaterial;
        gameObject.GetComponent<NavMeshModifier>().overrideArea = true;
        gameObject.GetComponent<NavMeshModifier>().area = 1;
        navmeshController.Refresh();
    }
}
