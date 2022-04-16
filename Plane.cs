using System;
using UnityEngine;

namespace Splines
{
    [Serializable]
    public struct Plane
    {
        [SerializeField] private Vector3 origin;
        [SerializeField] private Vector3 normal;
        [SerializeField] private Vector3 offset;
        
        public Vector3 Origin
        {
            get => origin;
            set => origin = value;
        }

        public Vector3 Normal
        {
            get => normal;
            set => normal = value;
        }
        
        public Vector3 Offset
        {
            get => offset;
            set => offset = value;
        }
        
        public Vector3 Constrain(Vector3 point)
        {
            var v = point - origin;
            var d = Vector3.Project(v, normal.normalized);
            
            return point - d + offset;
        }
    }
}
