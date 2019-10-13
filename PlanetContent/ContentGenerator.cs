using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class ContentGenerator : MonoBehaviour
{
    List<PointElevationValue> viablePoints = new List<PointElevationValue>();
    ResourceManager resourceManager => GameController.Instance.GetComponentInChildren<ResourceManager>();
    GameObject neutralSpaceEnemyContainer;
    Utilities Utils = new Utilities();
    EntityManager entityManager;

    public void GenerateContent(MeshFilter[] meshFilters, List<PointElevationValue> pointElevationValues, List<Spawnable> spawnables, MinMax minMax, Transform planet)
    {
        // find all viable points below, pick one at random with FindValidSpot
        float maxMagnitude = minMax.Max - minMax.Min;
        viablePoints = pointElevationValues.FindAll(point =>
        {
            float pointMagnitude = point.value - minMax.Min;
            float magRatio = pointMagnitude / maxMagnitude;
            return magRatio < 0.5f && magRatio > 0.1f;
        });
        foreach (MeshFilter filter in meshFilters)
        {
            foreach (Spawnable spawn in spawnables)
            {
                switch (spawn.spawnType)
                {
                    case Spawnable.SpawnType.Unit:
                        SpawnNeutralEnemies(spawn, planet);
                        break;
                    default:
                        SpawnContent(spawn, planet);
                        break;
                }

            }
        }
    }
    void SpawnContent(Spawnable spawn, Transform planet)
    {
        GameObject spawnObjContainer = new GameObject("ContentContainer");
        spawnObjContainer.transform.SetParent(planet);
        for (int i = 0; i < spawn.quantity; i++)
        {
            Vector3 validPoint = FindValidSpot();
            GameObject spawned = Instantiate<GameObject>(spawn.spawn);
            spawned.transform.localScale *= Random.Range(0.85f, 1.15f);
            spawned.transform.SetParent(planet);
            spawned.transform.localPosition = validPoint;
            var dir = validPoint - planet.position;
            spawned.transform.LookAt(planet.position, dir);
            spawned.transform.Rotate(new Vector3(0, 0, 1), Random.Range(0, 360));
            //spawned.transform.Translate(0, 5, 0, Space.Self);
            if (spawn.spawnType == Spawnable.SpawnType.Resource)
            {
                IResource resource = spawned.GetComponent<IResource>();
                if (resource != null)
                {
                    resource.ResourceReady(planet.parent.transform);
                }
            }
        }
    }
    void SpawnNeutralEnemies(Spawnable spawn, Transform planet)
    {
        //GameObject spawnObjContainer = new GameObject("EnemyContainer");
        //spawnObjContainer.transform.SetParent(planet);
        entityManager = World.Active.EntityManager;
        if (planet == null)
        {
            for (int i = 0; i < spawn.quantity; i++)
            {
                GameObject spawned = Instantiate<GameObject>(spawn.spawn);
                //ECS experiment!

                // spawned.transform.SetParent(neutralSpaceEnemyContainer.transform);
                spawned.transform.position = Utils.FindSpawnPoint(Vector3.zero, 1500, 300, false);
                spawned.transform.localEulerAngles = new Vector3(0, -90, 0);
                GlobalEventManager.OnUnitSpawned(spawned.transform, 0, null);
            }
        }
        else 
        {
            //TODO: reference prefab entity to ensure shared render meshes
            Entity entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(spawn.spawn, World.Active);
            for (int i = 0; i < spawn.quantity; i++)
            {
                Vector3 validPoint = FindValidSpot();
                // GameObject spawned = Instantiate<GameObject>(spawn.spawn);
                // Destroy(spawned);
                var instance = entityManager.Instantiate(entity);
                // spawned.transform.position = validPoint;
                //spawned.transform.SetParent(spawnObjContainer.transform);
                // spawned.transform.SetParent(planet);
                var dir = validPoint - planet.position;
                //spawned.transform.LookAt(planet.position, dir);
                // spawned.transform.Rotate(new Vector3(0, 0, 1), Random.Range(0, 360));
                //spawned.transform.Translate(0, 5, 0, Space.Self);
                // Vector3 newPos = spawned.transform.position - spawned.transform.up * 50f;
                // spawned.transform.localPosition = newPos;
                // GlobalEventManager.OnUnitSpawned(spawned.transform, 0, planet.parent.transform);
                entityManager.AddComponent(instance, typeof(Unit));
                entityManager.AddComponentData(instance, new Attract{ attractPoint = planet.position });
                entityManager.AddComponentData(instance, new AlignToPlanet{ Value = planet.position });
                entityManager.SetComponentData(instance, new Translation{Value = planet.TransformPoint(validPoint)});
            }
            entityManager.DestroyEntity(entity);
        }
    }

    public void GenerateSpaceContent(List<Spawnable> spawnables)
    {
        neutralSpaceEnemyContainer = new GameObject("Neutral Space Mobs");
            foreach (Spawnable spawn in spawnables)
            {
                switch (spawn.spawnType)
                {
                    case Spawnable.SpawnType.Unit:
                        SpawnNeutralEnemies(spawn, null);
                        break;
                    case Spawnable.SpawnType.Spawner:
                        SpawnNeutralSpawners(spawn, null);
                        break;
                    default:
                        SpawnContent(spawn, null);
                        break;
                }
            }

    }

    public void SpawnNeutralSpawners(Spawnable spawn, Transform planet)
    {
        
        if (planet == null)
        {
            for (int i = 0; i < spawn.quantity; i++)
            {
                GameObject spawned = Instantiate<GameObject>(spawn.spawn);
                // spawned.transform.SetParent(neutralSpaceEnemyContainer.transform);
                spawned.transform.position = Utils.FindSpawnPoint(Vector3.zero, 3000, 600, false);
                // spawned.transform.localEulerAngles = new Vector3(0, -90, 0);
                GlobalEventManager.OnStructureBuild(spawned.transform, 0, null);
                spawned.GetComponent<IBuildable>().OnStructureBuild(null);
            }
        }
        else 
        {
            for (int i = 0; i < spawn.quantity; i++)
            {
                Vector3 validPoint = FindValidSpot();
                GameObject spawned = Instantiate<GameObject>(spawn.spawn);
                //spawned.transform.localScale *= Random.Range(0.85f, 1.15f);
                spawned.transform.position = validPoint;
                //spawned.transform.SetParent(spawnObjContainer.transform);
                spawned.transform.SetParent(planet);
                var dir = validPoint - planet.position;
                //spawned.transform.LookAt(planet.position, dir);
                spawned.transform.Rotate(new Vector3(0, 0, 1), Random.Range(0, 360));
                //spawned.transform.Translate(0, 5, 0, Space.Self);
                Vector3 newPos = spawned.transform.position - spawned.transform.up * 50f;
                spawned.transform.localPosition = newPos;
                GlobalEventManager.OnStructureBuild(spawned.transform, 0, planet.parent.transform);
            }
        }
    }

    public Vector3 FindValidSpot()
    {

        Vector3 point = viablePoints[Random.Range(0, viablePoints.Count - 1)].position;

        // add check for collisions here
        return point;
    }
}
