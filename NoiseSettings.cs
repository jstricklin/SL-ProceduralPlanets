using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings {

    public enum FilterType { Simple, Rigid };
    public FilterType filterType;

    [ConditionalHide("filterType", 1)]
    public RigidNoiseSettings rigidNoiseSettings;
    [ConditionalHide("filterType", 0)]
    public SimpleNoiseSettings simpleNoiseSettings;
    
    [System.Serializable]
    public class SimpleNoiseSettings {
    
        public float minValue;
        public float strength = 1;
        [Range(1, 8)]
        public float numLayers = 1;
        public float baseRoughness = 1;
        public float roughness = 2;
        public float persistence = 0.5f;
        public Vector3 center;
        }

    [System.Serializable]
    public class RigidNoiseSettings : SimpleNoiseSettings {

        public float weightMultiplier = .8f;
        }
    }
