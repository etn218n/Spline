using System;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    [Serializable]
    public abstract class SplineComputer
    {
        [SerializeField] protected Plane plane;
        [SerializeField] protected float spacing;
        
        [HideInInspector]
        [SerializeField] protected List<Vector3> splinePoints;
        [HideInInspector]
        [SerializeField] protected List<Vector3> controlPoints;

        public Plane Plane => plane;
        public List<Vector3> SplinePoints => splinePoints;
        public int NumberOfSplinePoints => splinePoints.Count;
        public int NumberOfControlPoints => controlPoints.Count;
        public bool IsEmpty => splinePoints.Count == 0;

        public float Spacing
        {
            get => spacing;
            set => spacing = Mathf.Clamp(value, 0f, Mathf.Infinity);
        }

        protected SplineComputer()
        {
            splinePoints  = new List<Vector3>(200);
            controlPoints = new List<Vector3>(40);
            
            spacing = Mathf.Clamp(spacing, 0f, Mathf.Infinity);
        }
        
        protected SplineComputer(Plane plane, float spacing)
        {
            splinePoints  = new List<Vector3>(200);
            controlPoints = new List<Vector3>(40);
            
            this.plane   = plane;
            this.spacing = Mathf.Clamp(spacing, 0f, Mathf.Infinity);
        }
        
        public virtual void Clear()
        {
            splinePoints.Clear();
            controlPoints.Clear();
        }

        public int FindNearestPointIndexTo(Vector3 point)
        {
            var nearestPointIndex = 0;
            var nearestDistanceSoFar = Vector3.Distance(point, splinePoints[0]);

            for (int i = 1; i < splinePoints.Count; i++)
            {
                var newDistance = Vector3.Distance(point, splinePoints[i]);
                
                if (newDistance < nearestDistanceSoFar)
                {
                    nearestPointIndex = i;
                    nearestDistanceSoFar = newDistance;
                }
            }

            return nearestPointIndex;
        }

        public Vector3 FindNearestPointTo(Vector3 point)
        {
            var nearestPointIndex = FindNearestPointIndexTo(point);
            
            return splinePoints[nearestPointIndex];
        }
        
        public abstract void AddControlPoint(Vector3 newControlPoint);
        public abstract void MoveControlPoint(int index, Vector3 newPosition);
        public abstract void ForEachControlPoint(Action<Vector3, int> action);
        public abstract void RemoveLastSegment();
        public abstract void GeneratePoints();
        public abstract void GeneratePoints(List<Vector3> newControlPoints);
    }
}