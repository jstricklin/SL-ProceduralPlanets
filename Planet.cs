using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class Planet : MonoBehaviour
{

    [Range(2, 256)]
    public int resolution = 10;
    //ecs
    Entity ecsColliders;
    EntityManager entityManager;
    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColorGenerator colorGenerator = new ColorGenerator();
    ContentGenerator contentGenerator = new ContentGenerator();

    List<Spawnable> toSpawn;
    [SerializeField, HideInInspector]
    public MeshFilter[] meshFilters;
    //public List<Spawnable> spawnables = new List<Spawnable>();

    [SerializeField, HideInInspector]
    private UnityEngine.MeshCollider[] colliders;
    private GameObject collContainer;
    // nav mesh generation 
    [SerializeField]
    NavMeshSurface navMeshSurface;
    NavMeshGenerator navMeshGenerator = new NavMeshGenerator();

    public ColorSettings colorSettings;
    public ShapeSettings shapeSettings;

    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool colorSettingsFoldout;

    public bool autoUpdate = true;

    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back }
    public FaceRenderMask faceRenderMask;
    [ConditionalHide("filterType", 0)]
    public TerrainFace[] terrainFaces;

    UnityEngine.Material planetMat;

    private void Start()
    {
        //GeneratePlanet();
    }

    void Initialize()
    {
        //RandomizeSettings();
        shapeGenerator.UpdateSettings(shapeSettings);
        colorGenerator.UpdateSettings(colorSettings);

        if (meshFilters == null || meshFilters.Length == 0)
        {

            meshFilters = new MeshFilter[6];
        }

        terrainFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;
                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colorSettings.planetMaterial;
            terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }
    }

    public void GeneratePlanet()
    {
        Initialize();
        GenerateMesh();
        //GenerateColliders();
        GenerateColors();
        //GenerateContent();
        //GenerateNavMesh();
    }


    public void OnShapeSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();
            GenerateMesh();
            //GenerateColliders();
        }
    }

    public void OnColorSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();
            GenerateColors();
        }
    }

    void GenerateMesh()
    {
        //Debug.Log("generating mesh");
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
            }
        }
        colorGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    void GenerateColors()
    {
        colorGenerator.UpdateColors();
    }
    public void GenerateColliders()
    {
        if (colliders != null)
        {
            //Debug.Log("colliders not null!" + colliders);
            DestroyImmediate(collContainer);
        }
        colliders = new UnityEngine.MeshCollider[meshFilters.Length];
        collContainer = new GameObject("collContainer");
        //collContainer.transform.position = transform.position;
        collContainer.transform.parent = transform;
        for (int i = 0; i < colliders.Length; i++)
        {

            GameObject collider = new GameObject("collider");
            collider.layer = 8;
            collider.transform.localPosition = Vector3.zero;
            collider.transform.parent = collContainer.transform;
            colliders[i] = collider.AddComponent<UnityEngine.MeshCollider>();
            colliders[i].sharedMesh = meshFilters[i].sharedMesh;
        }
        // GenerateECSColliders(colliders);
    }
    public void GenerateECSColliders()
    {
        // //ecs
        entityManager = World.Active.EntityManager;
        ecsColliders = entityManager.CreateEntity();
        entityManager.SetName(ecsColliders, $"{this.gameObject.name} Colliders");
        entityManager.AddComponentData(ecsColliders, new Translation());
        // entityManager.SetComponentData(ecsColliders, new Translation { Value = new float3(0,0,0)});
        entityManager.AddComponentData(ecsColliders, new LocalToWorld());
        // var localToWorld = entityManager.GetComponentData<LocalToWorld>(ecsColliders);
        // var ecsColliders = new Unity.Physics.MeshCollider[meshFilters.Length];
        // //
        //ecs
        for (int i = 0; i < colliders.Length; i++)
        {
            var phys = colliders[i].gameObject.AddComponent<PhysicsShapeAuthoring>();
            phys.SetMesh(meshFilters[i].mesh);
            var ecsColl = GameObjectConversionUtility.ConvertGameObjectHierarchy(colliders[i].gameObject, World.Active);
            var localParent = entityManager.GetComponentData<LocalToWorld>(ecsColliders).Value;
            PhysicsCollider coll = entityManager.GetComponentData<PhysicsCollider>(ecsColl);
            unsafe {  Unity.Physics.Authoring.DisplayBodyColliders.DrawComponent.BuildDebugDisplayMesh(coll.ColliderPtr); }
            entityManager.AddComponent(ecsColl, typeof(PlanetCollider));
            entityManager.AddComponentData(ecsColl, new LocalToParent { Value = localParent });
            entityManager.AddComponentData(ecsColl, new Parent { Value = ecsColliders });
        }
    }
    public void PrepContent(List<Spawnable> spawnables)
    {
        toSpawn = spawnables;
        Invoke("GenerateContent", 0.1f);
    }
    public void GenerateContent()
    {
        // Debug.Log("generating content");
        contentGenerator.GenerateContent(meshFilters, shapeGenerator.pointElevationValues, toSpawn, shapeGenerator.elevationMinMax, gameObject.transform);
    }
    public void GenerateNavMesh()
    {
        //NavMeshSurface navMesh = 
        navMeshGenerator.GenerateNavMesh(this.transform, navMeshSurface);
    }
    //ecs
    public void SyncECSColliders(float3 pos)
    {
        entityManager.SetComponentData(ecsColliders, new Translation { Value = pos });
    }
}
