using UnityEngine;

public class BuzzsawController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 200;
    [SerializeField] private float travelSpeed = 3f;
    [SerializeField] private float travelDistance = 3;
    [Range(-1, 1)]
    [SerializeField] private int directionSign = 1;
    private Vector3 startPosition;
    private MeshRenderer renderer;
    void Awake()
    {
        renderer = GetComponentInChildren<MeshRenderer>();
        startPosition = transform.position;
    }
    void Start()
    {
        
    }

    void Update()
    {
        if (renderer.enabled)
        {
            renderer.transform.Rotate(rotationSpeed * Time.deltaTime * Vector3.down);
            Vector3 moveAmount = travelSpeed * Time.deltaTime * transform.right;
            if (directionSign == 1)
            {
                if (Vector3.Distance(transform.position+moveAmount,startPosition) > travelDistance)
                {
                    directionSign = -1;
                    transform.position -= moveAmount;
                }
                else transform.position += moveAmount;
            }
            else if (directionSign == -1)
            {
                if (Vector3.Distance(transform.position-moveAmount,startPosition) > travelDistance)
                {
                    directionSign = 1;
                    transform.position += moveAmount;
                }
                else transform.position -= moveAmount;
            }
        }
    }
}
