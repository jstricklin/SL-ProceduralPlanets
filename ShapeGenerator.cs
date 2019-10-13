using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PointElevationValue
{
    public Vector3 position;
    public float value;
    public PointElevationValue(Vector3 pos, float val)
    {
        this.position = pos;
        this.value = val;
    }
}

public class ShapeGenerator {
    public List<PointElevationValue> pointElevationValues = new List<PointElevationValue>();
    ShapeSettings settings;
    public INoiseFilter[] noiseFilters;
    public MinMax elevationMinMax;

    public void UpdateSettings(ShapeSettings settings) {
        this.settings = settings;
        noiseFilters = new INoiseFilter[settings.noiseLayers.Length];
        for (int i = 0; i < noiseFilters.Length; i++){
            noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(settings.noiseLayers[i].noiseSettings);
        }
        elevationMinMax = new MinMax();
    }

    public Vector3 CalculatePointOnPlanet(Vector3 pointOnUnitSphere){
        float elevation = 0;
        float firstLayerValue = 0;
        if (noiseFilters.Length > 0){
            firstLayerValue = noiseFilters[0].Evaluate(pointOnUnitSphere);
            if (settings.noiseLayers[0].enabled){
                elevation = firstLayerValue;
                if (settings.noiseLayers[0].noiseSettings.filterType == NoiseSettings.FilterType.Simple) {
                }
            }
        }
        for (int i = 1; i < noiseFilters.Length; i++){
            if (settings.noiseLayers[i].enabled){
                float mask = (settings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
                elevation += noiseFilters[i].Evaluate(pointOnUnitSphere) * mask;
            }
        }
        elevation = settings.planetRadius * (1 + elevation);
        elevationMinMax.AddValue(elevation);
        PointElevationValue pointVal = new PointElevationValue(pointOnUnitSphere * elevation, (pointOnUnitSphere * elevation).magnitude);
        pointElevationValues.Add(pointVal);

        return pointOnUnitSphere * elevation;
    }

}
