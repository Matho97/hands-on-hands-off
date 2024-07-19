using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GazeBimanual.Utils
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineRayIndicator : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        [SerializeField]
        private Color color = Color.red;
        [SerializeField]
        private float originVerticalOffset = -0.01f;

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        public void UpdatePosition(Ray ray, Vector3 rayEndPoint)
        {
            if (this.isActiveAndEnabled == false) return;

            bool isEndPointXInvalid = float.IsNaN(rayEndPoint.x);
            bool isEndPointYInvalid = float.IsNaN(rayEndPoint.y);
            bool isEndPointZInvalid = float.IsNaN(rayEndPoint.z);

            if (isEndPointXInvalid || isEndPointYInvalid || isEndPointZInvalid)
                return;

            lineRenderer.SetPosition(0, ray.origin + new Vector3(0f, originVerticalOffset, 0f));
            lineRenderer.SetPosition(1, rayEndPoint);
        }

        public void SetColor(Color color)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        void OnEnable()
        {
            lineRenderer.enabled = true;
            SetColor(color);
        }

        void OnDisable()
        {
            lineRenderer.enabled = false;
        }
    }
}
