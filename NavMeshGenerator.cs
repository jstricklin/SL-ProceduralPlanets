using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshGenerator 
{
    
    public void GenerateNavMesh(Transform center, NavMeshSurface navMeshSurface)
    {
        NavMeshData navMeshData = new NavMeshData();
        navMeshData.position = center.transform.position;
        navMeshSurface.UpdateNavMesh(navMeshData);
        navMeshSurface.BuildNavMesh();
    }
}
