using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GazeBimanual
{
    [ExecuteAlways]
    public class InteractionLinesIndicator : MonoBehaviour
    {
        [SerializeField]
        private GazePinchController gazePinchController;

        [SerializeField]
        private bool includeChildrenRenderers = true;
        [SerializeField]
        private float lineRendererLengthPercentage = 0.9f;
        [SerializeField]
        private LayerMask lineRendererRaycastLayerMask = Physics.DefaultRaycastLayers;

        [SerializeField]
        private Material lineRendererMaterial;
        public Material LineRendererMaterial
        {
            get { return lineRendererMaterial; }
            set
            {
                lineRendererMaterial = value;
                lineRenderer1.material = lineRendererMaterial;
                lineRenderer2.material = lineRendererMaterial;
            }
        }
        [SerializeField]
        private float lineRendererWidth = 0.002f;
        public float LineRendererWidth
        {
            get { return lineRendererWidth; }
            set
            {
                lineRendererWidth = value;
                lineRenderer1.startWidth = lineRendererWidth;
                lineRenderer1.endWidth = lineRendererWidth;
                lineRenderer2.startWidth = lineRendererWidth;
                lineRenderer2.endWidth = lineRendererWidth;
            }
        }
        [SerializeField]
        private Color lineRendererColor = new Color(0.2f, 0.2f, 0.2f, 0.75f);
        public Color LineRendererColor
        {
            get { return lineRendererColor; }
            set
            {
                lineRendererColor = value;
                lineRenderer1.startColor = lineRendererColor;
                lineRenderer1.endColor = lineRendererColor;
                lineRenderer2.startColor = lineRendererColor;
                lineRenderer2.endColor = lineRendererColor;
            }
        }

        private LineRenderer lineRenderer1;
        private LineRenderer lineRenderer2;

        void Awake()
        {
            if (gazePinchController == null)
                gazePinchController = this.GetComponent<GazePinchController>();
            if (gazePinchController == null)
                gazePinchController = this.GetComponentInParent<GazePinchController>();
        }

        void Start()
        {
            if (Application.isPlaying == false)
                return;

            if (gazePinchController == null)
            {
                Debug.LogError("InteractionLinesIndicator should have a GazePinchController reference");
                this.enabled = false;
                return;
            }

            if (lineRendererMaterial == null)
                lineRendererMaterial = new Material(Shader.Find("GazeBimanual/LineRenderer"));

            GameObject lineRendererParent = new GameObject("LineRenderers");
            lineRendererParent.transform.SetParent(this.transform);

            GameObject lineRenderer1Object = new GameObject("LineRenderer1");
            lineRenderer1Object.transform.SetParent(lineRendererParent.transform);
            lineRenderer1 = lineRenderer1Object.AddComponent<LineRenderer>();

            GameObject lineRenderer2Object = new GameObject("LineRenderer2");
            lineRenderer2Object.transform.SetParent(lineRendererParent.transform);
            lineRenderer2 = lineRenderer2Object.AddComponent<LineRenderer>();

            LineRendererMaterial = lineRendererMaterial;
            LineRendererWidth = lineRendererWidth;
            LineRendererColor = lineRendererColor;
        }

        void Update()
        {
            if (Application.isPlaying == false)
                return;

            if (gazePinchController == null || gazePinchController.GazePinchObject == null)
            {
                lineRenderer1.enabled = false;
                lineRenderer2.enabled = false;
                return;
            }

            lineRenderer1.enabled = true;
            lineRenderer2.enabled = true;

            lineRenderer1.SetPosition(0, gazePinchController.FingerTipPosition);
            lineRenderer2.SetPosition(0, gazePinchController.FingerTipPosition);

            // Get gaze pinch object combined bounds
            Bounds combinedBounds = gazePinchController.GazePinchObject.GetComponent<Renderer>().bounds;
            if (includeChildrenRenderers)
            {
                Renderer[] renderers = gazePinchController.GazePinchObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer childRenderer in renderers)
                {
                    if (childRenderer != gazePinchController.GazePinchObject.GetComponent<Renderer>())
                        combinedBounds.Encapsulate(childRenderer.bounds);
                }
            }

            // Set line renderer position to the edge of the combined bounds
            // Vector3 boundsEdge1 = GetRotatedBoundsEdge(combinedBounds, new Vector3(combinedBounds.extents.x, 0, 0), gazePinchController.FingerTipPosition);
            Vector3 boundsEdge1 = combinedBounds.center + new Vector3(combinedBounds.extents.x, 0, 0);
            Vector3 directionFromFingerTipToEdge1 = boundsEdge1 - gazePinchController.FingerTipPosition;
            Vector3 finalEdge1Position = gazePinchController.FingerTipPosition;

            bool hitThroughBoundsEdge1 = Physics.Raycast(gazePinchController.FingerTipPosition, directionFromFingerTipToEdge1, out RaycastHit hitInfo, directionFromFingerTipToEdge1.magnitude, lineRendererRaycastLayerMask);
            if (hitThroughBoundsEdge1)
                finalEdge1Position += directionFromFingerTipToEdge1.normalized * hitInfo.distance * lineRendererLengthPercentage;
            else
                finalEdge1Position += directionFromFingerTipToEdge1.normalized * directionFromFingerTipToEdge1.magnitude * lineRendererLengthPercentage;
            lineRenderer1.SetPosition(1, finalEdge1Position);

            // Vector3 boundsEdge2 = GetRotatedBoundsEdge(combinedBounds, new Vector3(-combinedBounds.extents.x, 0, 0), gazePinchController.FingerTipPosition);
            Vector3 boundsEdge2 = combinedBounds.center + new Vector3(-combinedBounds.extents.x, 0, 0);
            Vector3 directionFromFingerTipToEdge2 = boundsEdge2 - gazePinchController.FingerTipPosition;
            Vector3 finalEdge2Position = gazePinchController.FingerTipPosition;

            bool hitThroughBoundsEdge2 = Physics.Raycast(gazePinchController.FingerTipPosition, directionFromFingerTipToEdge2, out RaycastHit hitInfo2, directionFromFingerTipToEdge2.magnitude, lineRendererRaycastLayerMask);
            if (hitThroughBoundsEdge2)
                finalEdge2Position += directionFromFingerTipToEdge2.normalized * hitInfo2.distance * lineRendererLengthPercentage;
            else
                finalEdge2Position += directionFromFingerTipToEdge2.normalized * directionFromFingerTipToEdge2.magnitude * lineRendererLengthPercentage;
            lineRenderer2.SetPosition(1, finalEdge2Position);
        }
    }
}
