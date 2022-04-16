using UnityEngine;

namespace Splines
{
    [RequireComponent(typeof(LineRenderer))]
    public class SplineVisualizer : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Spline spline;
        
        [Header("Settings")]
        [SerializeField] private bool showPoints;
        [SerializeField] private float pointSize;
        
        private LineRenderer lineRenderer;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();

            DrawSpline();
            
            spline.WhenChanged += DrawSpline;
        }

        private void OnDrawGizmosSelected()
        {
            if (showPoints && spline.IsEmpty)
            {
                Gizmos.color = Color.red;
                
                foreach (var point in spline.SplinePoints)
                    Gizmos.DrawSphere(point, pointSize);
            }
        }

        public void DrawSpline()
        {
            var points = spline.SplinePoints;
            
            lineRenderer.positionCount = points.Count;

            for (int i = 0; i < points.Count; i++)
                lineRenderer.SetPosition(i, points[i]);
        }
    }
}