using UnityEngine;

namespace Utilities
{
    public static class Vector3Extensions
    {
        public static bool AproxEquals(this Vector3 a, Vector3 b, float tolerance = 1e-6f)
        {
            // Compare squared lengths to avoid the sqrt cost of Vector3.Distance
            return (a - b).sqrMagnitude < tolerance * tolerance;
        }
    }
}