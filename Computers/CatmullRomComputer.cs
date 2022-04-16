using System;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    [Serializable]
    public class CatmullRomComputer : SplineComputer
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

        public CatmullRomComputer(float alpha, float tension, Plane plane, float spacing) : base(plane, spacing)
        {
            this.alpha   = Mathf.Clamp01(alpha);
            this.tension = Mathf.Clamp01(tension);
        }
        
        public override void AddControlPoint(Vector3 newControlPoint)
        {
            newControlPoint = plane.Constrain(newControlPoint);

            if (controlPoints.Contains(newControlPoint))
                return;

            if (controlPoints.Count == 1)
            {
                controlPoints.Add(newControlPoint);
                
                var v  = (controlPoints[1] - controlPoints[0]).normalized;
                var p0 = controlPoints[0] + v * 2;
                var p3 = controlPoints[1] + v * 2;
                
                controlPoints.Insert(0, p0);
                controlPoints.Add(p3);
            }
            else
            {
                var insertIndex = Mathf.Clamp(controlPoints.Count - 1, 0, int.MaxValue);

                controlPoints.Insert(insertIndex, newControlPoint); 
            }
        }
        
        public override void MoveControlPoint(int index, Vector3 newPosition)
        {
            if (index < 1 || index > controlPoints.Count - 2)
                return;
            
            controlPoints[index] = plane.Constrain(newPosition);
        }

        public override void ForEachControlPoint(Action<Vector3, int> action)
        {
            for (int i = 1; i <= controlPoints.Count - 2; i++)
                action(controlPoints[i], i);
        }

        public override void RemoveLastSegment()
        {
            if (controlPoints.Count <= 4)
            {
                Clear();
                return;
            }
            
            controlPoints.RemoveAt(controlPoints.Count - 2);
        }

        public override void GeneratePoints()
        {
            if (controlPoints.Count < 4)
                return;

            splinePoints.Clear();
            
            if (spacing > 0f)
            {
                GenerateEvenlySpacedPoints();
                return;
            }

            for (int i = 1; i <= controlPoints.Count - 3; i++)
            {
                var p0 = controlPoints[i - 1];
                var p1 = controlPoints[i + 0];
                var p2 = controlPoints[i + 1];
                var p3 = controlPoints[i + 2];
                
                var t0 = 0f;
                var t1 = t0 + Mathf.Pow(Vector3.Distance(p0, p1), alpha);
                var t2 = t1 + Mathf.Pow(Vector3.Distance(p1, p2), alpha);
                var t3 = t2 + Mathf.Pow(Vector3.Distance(p2, p3), alpha);

                var m1 = (1.0f - tension) * (t2 - t1) * ((p1 - p0) / (t1 - t0) - (p2 - p0) / (t2 - t0) + (p2 - p1) / (t2 - t1));
                var m2 = (1.0f - tension) * (t2 - t1) * ((p2 - p1) / (t2 - t1) - (p3 - p1) / (t3 - t1) + (p3 - p2) / (t3 - t2));

                var a =  2.0f * (p1 - p2) + m1 + m2;
                var b = -3.0f * (p1 - p2) - m1 - m1 - m2;
                var c = m1;
                var d = p1;
                
                GeneraSegmentPoints(a, b, c, d);
            }
        }
        
        public override void GeneratePoints(List<Vector3> newControlPoints)
        {
            controlPoints.Clear();
            
            foreach (var point in newControlPoints)
                AddControlPoint(point);

            GeneratePoints();
        }

        public void GeneraSegmentPoints(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var t = 0.0f;
            var step = 1f / 10f;

            while (t < 1f)
            {
                var point = (a * t * t * t) + (b * t * t) + (c * t) + d;
                splinePoints.Add(point);
                
                t += step;
            }
            
            splinePoints.Add(a + b + c + d);
        }

        private void GenerateEvenlySpacedPoints()
        {
            splinePoints.Add(controlPoints[1]);

            var resolution = 10f;
            var previousPoint = controlPoints[1];
            var distanceSinceLastEvenPoint = 0f;

            for (int i = 1; i <= controlPoints.Count - 3; i++)
            {
                var p0 = controlPoints[i - 1];
                var p1 = controlPoints[i + 0];
                var p2 = controlPoints[i + 1];
                var p3 = controlPoints[i + 2];
                
                var t0 = 0f;
                var t1 = t0 + Mathf.Pow(Vector3.Distance(p0, p1), alpha);
                var t2 = t1 + Mathf.Pow(Vector3.Distance(p1, p2), alpha);
                var t3 = t2 + Mathf.Pow(Vector3.Distance(p2, p3), alpha);

                var m1 = (1f - tension) * (t2 - t1) * ((p1 - p0) / (t1 - t0) - (p2 - p0) / (t2 - t0) + (p2 - p1) / (t2 - t1));
                var m2 = (1f - tension) * (t2 - t1) * ((p2 - p1) / (t2 - t1) - (p3 - p1) / (t3 - t1) + (p3 - p2) / (t3 - t2));

                var a =  2f * (p1 - p2) + m1 + m2;
                var b = -3f * (p1 - p2) - m1 - m1 - m2;
                var c = m1;
                var d = p1;

                var t = 0f;
                var estimatedSegmentLength = EstimateSegmentLength(a, b, c, d, 0.25f);
                var divisions = Mathf.CeilToInt(estimatedSegmentLength * resolution * 10);
                var stepSize  = 1f / divisions;
                
                while (t < 1f)
                {
                    t += stepSize;
                    
                    var pointOnCurve = (a * t * t * t) + (b * t * t) + (c * t) + d;
                    distanceSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

                    while (distanceSinceLastEvenPoint >= spacing)
                    {
                        var overshootDistance    = distanceSinceLastEvenPoint - spacing;
                        var newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDistance;
                        
                        splinePoints.Add(newEvenlySpacedPoint);

                        distanceSinceLastEvenPoint = overshootDistance;
                        previousPoint = newEvenlySpacedPoint;
                    }

                    previousPoint = pointOnCurve;
                }
                
                splinePoints.Add(a + b + c + d); // end point
            }
        }
        
        private float EstimateSegmentLength(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float stepSize)
        {
            var t = stepSize;
            var segmentLength = 0f;
            var previousPoint = d;
            
            while (t < 1f)
            {
                var point = (a * t * t * t) + (b * t * t) + (c * t) + d;

                segmentLength += Vector3.Distance(point, previousPoint);
                previousPoint = point;
                t += stepSize;
            }

            var endPoint = a + b + c + d;
            segmentLength += Vector3.Distance(endPoint, previousPoint);

            return segmentLength;
        }
    }
}