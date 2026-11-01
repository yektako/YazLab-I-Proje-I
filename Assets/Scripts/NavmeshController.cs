using System;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class NavmeshController : MonoBehaviour
{
    private NavMeshSurface navmeshSurface;
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
        navmeshSurface.UpdateNavMesh(navmeshSurface.navMeshData);
    }
    public void Bake()
    {
        navmeshSurface.BuildNavMesh();
    }
}
