using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Line : MonoBehaviour
{
    public LineRenderer lineRenderer;
    [SerializeField]
    private float _lineWidth = 0.01f;
    public float LineWidth
    {
        get => _lineWidth;
        set
        {
            _lineWidth = value;
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = _lineWidth;
                lineRenderer.endWidth = _lineWidth;
                lineRenderer.widthCurve = AnimationCurve.Constant(0, 1, _lineWidth);
            }
        }
    }

    public float minPointDistance = 0.005f;  // Minimum distance between points

    private List<Vector3> points;

    /// <summary>
    /// Initialize the line with a starting point in local space.
    /// Parent must be set before calling this.
    /// </summary>
    public void Initialize(Vector3 localStartPosition)
    {
        if (points == null)
            points = new List<Vector3>();
        else
            points.Clear();

        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = false;
        lineRenderer.alignment = LineAlignment.TransformZ;

        // Apply current LineWidth
        LineWidth = _lineWidth;

        AddPoint(localStartPosition);
    }


    private void AddPoint(Vector3 localPos)
    {
        points.Add(localPos);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(points.Count - 1, localPos);
    }

    /// <summary>
    /// Update the line with a new position in local space
    /// </summary>
    public void UpdateLine(Vector3 localPos)
    {
        if (points == null || points.Count == 0)
        {
            Initialize(localPos);
            return;
        }

        if (Vector3.Distance(points.Last(), localPos) >= minPointDistance)
        {
            AddPoint(localPos);
        }
    }

    /// <summary>
    /// Apply current lineWidth to the LineRenderer
    /// </summary>
    public void ApplyLineWidth()
    {
        lineRenderer.startWidth = LineWidth;
        lineRenderer.endWidth = LineWidth;
        lineRenderer.widthCurve = AnimationCurve.Constant(0, 1, LineWidth);
    }

    /// <summary>
    /// Reset line for object pooling reuse
    /// </summary>
    public void ResetLine()
    {
        points?.Clear();
        lineRenderer.positionCount = 0;
        lineRenderer.alignment = LineAlignment.TransformZ;

        // Apply current LineWidth
        LineWidth = _lineWidth;
    }

}










