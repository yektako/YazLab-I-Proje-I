using Unity.AI.Navigation;
using UnityEngine;

public class FallingFloorController : MonoBehaviour, IResettable, IDetectingController
{
    [SerializeField] private NavmeshController navmeshController;
    [Tooltip("Platform hangi Y koordinatına inecek")]
    [SerializeField] private float finalYPosition = 0;

    [Tooltip("Platform kaç saniyede düşecek")]
    [SerializeField] private float fallingSpeed = 1f;
    [SerializeField] private Material idleMaterial;
    [SerializeField] private Material triggeredMaterial;
    [SerializeField] private Material descendingMaterial;
    private MeshRenderer renderer;
    private NavMeshModifier modifier;
    private bool isDescending = false;
    private Vector3 startPosition;
    void Start()
    {
        renderer = gameObject.GetComponent<MeshRenderer>();
        modifier = gameObject.GetComponent<NavMeshModifier>();
        modifier.area = 1;
        idleMaterial = renderer.material;
        startPosition = transform.position;
    }

    void Update()
    {
        if (isDescending)
        {
            if (transform.position.y > finalYPosition)
            {
                transform.position -= fallingSpeed * Time.deltaTime * Vector3.up;
                if (transform.position.y < finalYPosition)
                transform.position = new Vector3(transform.position.x, finalYPosition, transform.position.z);
            }
        }
    }
    public void ResetToStart()
    {
        isDescending = false;
        renderer.material = idleMaterial;
        transform.position = startPosition;
        modifier.overrideArea = false;
        navmeshController.Refresh();
    }
    public void DetectorTriggerEnter(Collider other)
    {
        if (!isDescending) renderer.material = triggeredMaterial;
    }
    public void DetectorTriggerExit(Collider other)
    {
        isDescending = true;
        renderer.material = descendingMaterial;
        modifier.overrideArea = true;
        navmeshController.Refresh();
    }
}
