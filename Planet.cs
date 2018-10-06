using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {

    [Range(2,256)]
    public int resolution = 10;

    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColorGenerator colorGenerator = new ColorGenerator();

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;

    [SerializeField, HideInInspector]
    MeshCollider[] colliders;

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

    TerrainFace[] terrainFaces;

    void Initialize(){

        shapeGenerator.UpdateSettings(shapeSettings);
        colorGenerator.UpdateSettings(colorSettings);

        if (meshFilters == null || meshFilters.Length == 0){

            meshFilters = new MeshFilter[6];
        }

        terrainFaces = new TerrainFace[6];

        Vector3[] directions = {Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back};

        for (int i = 0; i < 6; i++){
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

    public void GeneratePlanet(){
        Initialize();
        GenerateMesh();
        GenerateColliders();
        GenerateColors();
    }

    public void OnShapeSettingsUpdated(){
        if (autoUpdate)
        {
            Initialize();
            GenerateMesh();
        }
    }

    public void OnColorSettingsUpdated(){
        if (autoUpdate) {
            Initialize();
            GenerateColors();
        }
    } 

    void GenerateMesh(){
        for (int i = 0; i < 6; i++){
            if (meshFilters[i].gameObject.activeSelf){
                terrainFaces[i].ConstructMesh();
            }
        }
        colorGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    void GenerateColors(){
            colorGenerator.UpdateColors();
    }
    public void GenerateColliders()
    {
        if (colliders.Length == 0)
        {
            Debug.Log("colliders null");
            colliders = new MeshCollider[meshFilters.Length];
            GameObject collContainer = new GameObject("collContainer");
            collContainer.transform.parent = transform;
            for (int i = 0; i < colliders.Length; i++){

                GameObject collider = new GameObject("collider");
                collider.transform.parent = collContainer.transform;
                colliders[i] = collider.AddComponent<MeshCollider>();
            }
        }
        Debug.Log("colliders exist.");
        for (int i = 0; i < colliders.Length; i++){
            colliders[i].sharedMesh = meshFilters[i].sharedMesh;
        }
    }
}
