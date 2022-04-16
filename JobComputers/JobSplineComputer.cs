using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Splines
{
    [Serializable]
    public abstract class JobSplineComputer
    {
        [SerializeField] protected Plane plane;
        [SerializeField] protected float spacing;
        
        protected NativeList<Vector3> splinePoints;
        protected NativeList<Vector3> controlPoints;

        public Plane Plane => plane;
        public NativeList<Vector3> SplinePoints => splinePoints;
        public int NumberOfSplinePoints => splinePoints.Length;
        public int NumberOfControlPoints => controlPoints.Length;

        public float Spacing
        {
            get => spacing;
            set => spacing = Mathf.Clamp(value, 0f, Mathf.Infinity);
        }

        protected JobSplineComputer()
        {
            splinePoints  = new NativeList<Vector3>(200, Allocator.Persistent);
            controlPoints = new NativeList<Vector3>(40,  Allocator.Persistent);
            
            spacing = Mathf.Clamp(spacing, 0f, Mathf.Infinity);
        }
        
        protected JobSplineComputer(Plane plane, float spacing)
        {
            splinePoints  = new NativeList<Vector3>(200, Allocator.Persistent);
            controlPoints = new NativeList<Vector3>(40,  Allocator.Persistent);
            
            this.plane   = plane;
            this.spacing = Mathf.Clamp(spacing, 0f, Mathf.Infinity);
        }

        public virtual void Clear()
        {
            splinePoints.Clear();
            controlPoints.Clear();
        }

        public abstract void AddControlPoint(Vector3 newControlPoint);
        public abstract void AddControlPoints(List<Vector3> newControlPoints);
        public abstract void MoveControlPoint(int index, Vector3 newPosition);
        public abstract void ForEachControlPoint(Action<Vector3, int> action);
        public abstract void RemoveLastSegment();
        public abstract void GeneratePoints();
        public abstract void GeneratePoints(List<Vector3> newControlPoints);
        public abstract void GeneratePoints(NativeList<Vector3> newControlPoints);
        public abstract void Dispose();
        public abstract JobHandle CreateHandle();
    }
}