using System;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class NavmeshController : MonoBehaviour
{
    private NavMeshSurface navmeshSurface;
    private bool isInvoked = false;
    void Awake()
    {
        navmeshSurface = GetComponent<NavMeshSurface>();
    }
    void Start()
    {
        Bake();
    }
    void Update()
    {
    }
    public void Refresh()
    {
        if (!isInvoked)
        {
            isInvoked = true;
            Invoke(nameof(Bake), 1);
        }
    }
    public void Bake()
    {
        //navmeshSurface.BuildNavMesh();
        navmeshSurface.UpdateNavMesh(navmeshSurface.navMeshData);
        isInvoked = false;
    }
}
