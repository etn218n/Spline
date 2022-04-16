#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using UnityEngine;

namespace Splines
{
    #if UNITY_EDITOR
    [CustomEditor(typeof(CatmullRomSpline))]
    public class CatmullRomSplineEditor : Editor
    {
        private float handleScale = 0.2f;
        private CatmullRomSpline spline;

        private void OnEnable()
        {
            spline = (CatmullRomSpline)target;
        }  
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
        
            DrawDefaultInspector();
            EditorGUILayout.Space();
        
            if (GUILayout.Button("Add Segment"))
            {
                Undo.RecordObject(spline, "Create New");
                
                var endPoint = CalculateEndPoint();

                if (spline.IsEmpty)
                {
                    spline.AddControlPoint(spline.transform.position);
                    spline.AddControlPoint(endPoint);
                }
                else
                {
                    spline.AddControlPoint(endPoint);
                }
                
                spline.GeneratePoints();

                RecordObjectModification();
            }
            
            if (GUILayout.Button("Remove Last Segment"))
            {
                Undo.RecordObject(spline, "Remove Last Segment");
                
                spline.RemoveLastSegment();
                spline.GeneratePoints();

                RecordObjectModification();
            }

            if (GUILayout.Button("Clear"))
            {
                spline.Clear();
            }
        
            serializedObject.ApplyModifiedProperties();
        }
        
        private void OnSceneGUI()
        {
            if (!spline.IsEmpty && spline.NumberOfSplinePoints == 0)
                spline.GeneratePoints();

            DetectHandlesMovement();
            DrawCatmullRomCurve();
        }
        
        private Vector3 CalculateEndPoint()
        {
            if (spline.IsEmpty)
                return spline.transform.position + Vector3.one * HandleUtility.GetHandleSize(spline.transform.position) * 0.1f;
            
            return spline.SplinePoints[spline.NumberOfSplinePoints - 1] + Vector3.one * HandleUtility.GetHandleSize(spline.transform.position) * 0.1f;
        }

        private void DrawCatmullRomCurve()
        {
            Handles.color = Color.green;

            for (int i = 0; i <= spline.NumberOfSplinePoints - 2; i++)
                Handles.DrawLine(spline.SplinePoints[i], spline.SplinePoints[i + 1]);
        }
        
        private void DetectHandlesMovement()
        {
            spline.ForEachControlPoint(HandleControlPoint);
        }
        
        private void HandleControlPoint(Vector3 controlPoint, int index)
        {
            Handles.color = Color.red;
            
            var handleSize  = HandleUtility.GetHandleSize(controlPoint) * handleScale;
            var newPosition = Handles.FreeMoveHandle(controlPoint, Quaternion.identity, handleSize, Vector2.zero, Handles.SphereHandleCap);
            
            if (newPosition != controlPoint)
            {
                Undo.RecordObject(spline, "Move Control Point");
                
                spline.MoveControlPoint(index, newPosition);
                spline.GeneratePoints();

                RecordObjectModification();
            }
        }

        private void RecordObjectModification()
        {
            if (EditorApplication.isPlaying)
                return;
            
            EditorUtility.SetDirty(spline);
            EditorSceneManager.MarkSceneDirty(spline.gameObject.scene);
            PrefabUtility.RecordPrefabInstancePropertyModifications(spline);
        }
    }
    #endif
}
