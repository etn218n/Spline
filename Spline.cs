using System;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    public abstract class Spline : MonoBehaviour
    {
        public event Action WhenChanged;

        public abstract bool IsEmpty { get; }
        public abstract List<Vector3> SplinePoints { get; }
        public abstract int NumberOfSplinePoints { get; }
        public abstract int NumberOfControlPoints { get; }
        public abstract void AddControlPoint(Vector3 controlPoint);
        public abstract void MoveControlPoint(int index, Vector3 newPosition);
        public abstract void GeneratePoints();
        public abstract void GeneratePoints(List<Vector3> newControlPoints);
        
        public abstract void Clear();

        protected void InvokeChangeEvent()
        {
            WhenChanged?.Invoke();
        }
    }
}