using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace Splines
{
    [BurstCompile]
    public struct GenerateCatmullRomJob : IJob
    {
        [ReadOnly]
        public NativeList<Vector3> ControlPoints;
        public NativeList<Vector3> SplinePoints;

        public float Alpha;
        public float Tension;
        public float Spacing;

        public void Execute()
        {
            if (ControlPoints.Length < 4)
                return;

            SplinePoints.Clear();
            
            if (Spacing > 0)
            {
                GenerateEvenlySpacedPoints();
                return;
            }

            for (int i = 1; i <= ControlPoints.Length - 3; i++)
            {
                var p0 = ControlPoints[i - 1];
                var p1 = ControlPoints[i + 0];
                var p2 = ControlPoints[i + 1];
                var p3 = ControlPoints[i + 2];
                
                var t0 = 0f;
                var t1 = t0 + Mathf.Pow(Vector3.Distance(p0, p1), Alpha);
                var t2 = t1 + Mathf.Pow(Vector3.Distance(p1, p2), Alpha);
                var t3 = t2 + Mathf.Pow(Vector3.Distance(p2, p3), Alpha);

                var m1 = (1.0f - Tension) * (t2 - t1) * ((p1 - p0) / (t1 - t0) - (p2 - p0) / (t2 - t0) + (p2 - p1) / (t2 - t1));
                var m2 = (1.0f - Tension) * (t2 - t1) * ((p2 - p1) / (t2 - t1) - (p3 - p1) / (t3 - t1) + (p3 - p2) / (t3 - t2));

                var a =  2.0f * (p1 - p2) + m1 + m2;
                var b = -3.0f * (p1 - p2) - m1 - m1 - m2;
                var c = m1;
                var d = p1;
                
                GenerateSegmentPoints(a, b, c, d);
            }
        }

        private void GenerateEvenlySpacedPoints()
        {
            SplinePoints.Add(ControlPoints[1]);

            var resolution = 10f;
            var previousPoint = ControlPoints[1];
            var distanceSinceLastEvenPoint = 0f;

            for (int i = 1; i <= ControlPoints.Length - 3; i++)
            {
                var p0 = ControlPoints[i - 1];
                var p1 = ControlPoints[i + 0];
                var p2 = ControlPoints[i + 1];
                var p3 = ControlPoints[i + 2];
                
                var t0 = 0f;
                var t1 = t0 + Mathf.Pow(Vector3.Distance(p0, p1), Alpha);
                var t2 = t1 + Mathf.Pow(Vector3.Distance(p1, p2), Alpha);
                var t3 = t2 + Mathf.Pow(Vector3.Distance(p2, p3), Alpha);

                var m1 = (1f - Tension) * (t2 - t1) * ((p1 - p0) / (t1 - t0) - (p2 - p0) / (t2 - t0) + (p2 - p1) / (t2 - t1));
                var m2 = (1f - Tension) * (t2 - t1) * ((p2 - p1) / (t2 - t1) - (p3 - p1) / (t3 - t1) + (p3 - p2) / (t3 - t2));

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

                    while (distanceSinceLastEvenPoint >= Spacing)
                    {
                        var overshootDistance    = distanceSinceLastEvenPoint - Spacing;
                        var newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDistance;
                        
                        SplinePoints.Add(newEvenlySpacedPoint);

                        distanceSinceLastEvenPoint = overshootDistance;
                        previousPoint = newEvenlySpacedPoint;
                    }

                    previousPoint = pointOnCurve;
                }
                
                SplinePoints.Add(a + b + c + d); // end point
            }
        }
        
        public void GenerateSegmentPoints(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var t = 0.0f;
            var step = 1f / 10f;

            while (t < 1f)
            {
                var point = (a * t * t * t) + (b * t * t) + (c * t) + d;
                SplinePoints.Add(point);
                
                t += step;
            }
            
            SplinePoints.Add(a + b + c + d);
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