using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace Splines
{
    [Serializable]
    public class JobCatmullRomComputer : JobSplineComputer
    {
        [Range(0f, 1f)]
        [SerializeField] private float alpha;
        [Range(0f, 1f)]
        [SerializeField] private float tension;

        public float Alpha
        {
            get => alpha;
            set => alpha = Mathf.Clamp01(value);
        }

        public float Tension
        {
            get => tension;
            set => tension = Mathf.Clamp01(value);
        }

        public JobCatmullRomComputer(float alpha, float tension, Plane plane, float spacing = 0f) : base(plane, spacing)
        {
            this.alpha   = Mathf.Clamp01(alpha);
            this.tension = Mathf.Clamp01(tension);
        }
        
        public override void AddControlPoint(Vector3 newControlPoint)
        {
            newControlPoint = plane.Constrain(newControlPoint);

            if (controlPoints.Contains(newControlPoint))
                return;

            if (controlPoints.Length == 0)
            {
                controlPoints.Add(newControlPoint);
                return;
            }
            
            if (controlPoints.Length == 1)
            {
                controlPoints.Add(newControlPoint);
                
                var v  = (controlPoints[1] - controlPoints[0]).normalized;
                var p1 = controlPoints[0];
                var p2 = controlPoints[1];
                var p0 = controlPoints[0] + v * 2;
                var p3 = controlPoints[1] + v * 2;

                controlPoints.Clear();
                
                controlPoints.Add(p0);
                controlPoints.Add(p1);
                controlPoints.Add(p2);
                controlPoints.Add(p3);
            }
            else if (controlPoints.Length >= 4)
            {
                var lastPoint = controlPoints[controlPoints.Length - 1];

                controlPoints[controlPoints.Length - 1] = newControlPoint;

                controlPoints.Add(lastPoint);
            }
        }

        public override void AddControlPoints(List<Vector3> newControlPoints)
        {
            foreach (var controlPoint in newControlPoints)
                AddControlPoint(controlPoint);
        }

        public override void MoveControlPoint(int index, Vector3 newPosition)
        {
            if (index < 1 || index > controlPoints.Length - 2)
                return;
            
            controlPoints[index] = plane.Constrain(newPosition);
        }

        public override void ForEachControlPoint(Action<Vector3, int> action)
        {
            for (int i = 1; i <= controlPoints.Length - 2; i++)
                action(controlPoints[i], i);
        }

        public override void RemoveLastSegment()
        {
            if (controlPoints.Length <= 4)
            {
                Clear();
                return;
            }
            
            controlPoints.RemoveAt(controlPoints.Length - 2);
        }

        public override void GeneratePoints()
        {
            var handle = CreateHandle();
            
            handle.Complete();
        }

        public override void GeneratePoints(List<Vector3> newControlPoints)
        {
            controlPoints.Clear();
            
            foreach (var point in newControlPoints)
                AddControlPoint(point);
            
            GeneratePoints();
        }

        public override void GeneratePoints(NativeList<Vector3> newControlPoints)
        {
            controlPoints.Clear();
            
            foreach (var point in newControlPoints)
                AddControlPoint(point);

            GeneratePoints();
        }

        public override void Dispose()
        {
            splinePoints.Dispose();
            controlPoints.Dispose();
        }

        public override JobHandle CreateHandle()
        {
            var job = new GenerateCatmullRomJob
            {
                Alpha         = alpha,
                Tension       = tension,
                Spacing       = spacing,
                SplinePoints  = splinePoints,
                ControlPoints = controlPoints
            };

            return job.Schedule();
        }
    }
}