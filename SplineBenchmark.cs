using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using Debug  = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Splines
{
    public class SplineBenchmark : MonoBehaviour
    {
        [SerializeField] private int numberOfSplines       = 10;
        [SerializeField] private int numberOfControlPoints = 10;

        [SerializeField] private CatmullRomComputer regularCatmullRom;
        [NonSerialized]  private List<JobCatmullRomComputer> jobCatmullRoms;

        private void Awake()
        {
            jobCatmullRoms = new List<JobCatmullRomComputer>(numberOfSplines);
            
            for (int i = 0; i < numberOfSplines; i++)
            {
                var jobCatmullRom = new JobCatmullRomComputer(regularCatmullRom.Alpha, 
                                                              regularCatmullRom.Tension, 
                                                              regularCatmullRom.Plane, 
                                                              regularCatmullRom.Spacing);

                jobCatmullRoms.Add(jobCatmullRom);
            }
        }

        private void Start()
        {
            var randomControlPoints1 = GenerateRandomControlPoints(numberOfControlPoints);
            BenchmarkJobSpline(randomControlPoints1); 
            BenchmarkRegularSpline(randomControlPoints1);
            
            var randomControlPoints2 = GenerateRandomControlPoints(numberOfControlPoints);
            BenchmarkJobSpline(randomControlPoints2); 
            BenchmarkRegularSpline(randomControlPoints2);
            
            var randomControlPoints3 = GenerateRandomControlPoints(numberOfControlPoints);
            BenchmarkJobSpline(randomControlPoints3); 
            BenchmarkRegularSpline(randomControlPoints3);
        }

        private void BenchmarkJobSpline(List<Vector3> controlPoints)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            var pointCount = 0;
            var handles = new NativeList<JobHandle>(numberOfSplines, Allocator.Temp);
            
            for (int i = 0; i < numberOfSplines; i++)
            {
                jobCatmullRoms[i].Clear();
                
                foreach (var point in controlPoints)
                    jobCatmullRoms[i].AddControlPoint(point);

                handles.Add(jobCatmullRoms[i].CreateHandle());
            }

            JobHandle.CompleteAll(handles);
            handles.Dispose();

            foreach (var jobCatmullRom in jobCatmullRoms)
                pointCount += jobCatmullRom.SplinePoints.Length;

            stopWatch.Stop();
            Debug.Log($"Job: generated {pointCount} points in {stopWatch.Elapsed.TotalMilliseconds} ms");
        }

        private void BenchmarkRegularSpline(List<Vector3> controlPoints)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            var pointCount = 0;
            
            for (int i = 0; i < numberOfSplines; i++)
            {
                regularCatmullRom.GeneratePoints(controlPoints);
                
                pointCount += regularCatmullRom.NumberOfSplinePoints;
            }
            
            stopWatch.Stop();
            Debug.Log($"Regular: generated {pointCount} points in {stopWatch.Elapsed.TotalMilliseconds} ms");
        }

        private List<Vector3> GenerateRandomControlPoints(int count)
        {
            var pointList = new List<Vector3>(1000);

            for (int i = 0; i < count; i++)
            {
                var randomPoint = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0f);
                
                pointList.Add(randomPoint);
            }

            return pointList;
        }

        private void OnDestroy()
        {
            jobCatmullRoms.ForEach(job => job.Dispose());
        }
    }
}