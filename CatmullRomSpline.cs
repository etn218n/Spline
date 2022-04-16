using System;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    public class CatmullRomSpline : Spline
    {
        [SerializeField] 
        private CatmullRomComputer catmullRomComputer;

        public override bool IsEmpty => catmullRomComputer.IsEmpty;
        public override List<Vector3> SplinePoints => catmullRomComputer.SplinePoints;
        public override int NumberOfSplinePoints => catmullRomComputer.NumberOfSplinePoints;
        public override int NumberOfControlPoints => catmullRomComputer.NumberOfControlPoints;

        protected void Awake()
        {
            catmullRomComputer.GeneratePoints();
        }

        public void RemoveLastSegment()
        {
            catmullRomComputer.RemoveLastSegment();
            
            InvokeChangeEvent();
        }
        
        public override void AddControlPoint(Vector3 controlPoint)
        {
            catmullRomComputer.AddControlPoint(controlPoint);
            catmullRomComputer.GeneratePoints();
            
            InvokeChangeEvent();
        }
        
        public override void MoveControlPoint(int index, Vector3 newPosition)
        {
            catmullRomComputer.MoveControlPoint(index, newPosition);
            
            InvokeChangeEvent();
        }
        
        public void ForEachControlPoint(Action<Vector3, int> action)
        {
            catmullRomComputer.ForEachControlPoint(action);
        }

        public override void GeneratePoints()
        {
            catmullRomComputer.GeneratePoints();
            
            InvokeChangeEvent();
        }

        public override void GeneratePoints(List<Vector3> newControlPoints)
        {
            catmullRomComputer.GeneratePoints(newControlPoints);
            
            InvokeChangeEvent();
        }

        public override void Clear()
        {
            catmullRomComputer.Clear();
            
            InvokeChangeEvent();
        }
    }
}
