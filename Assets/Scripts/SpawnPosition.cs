using System;
using UnityEngine;



[Serializable]
public struct SpawnPosition {

    public Vector3 Center;
    [Min((0f))]
    public float Radius;
    [Min((0f))]
    public float Distance;

}