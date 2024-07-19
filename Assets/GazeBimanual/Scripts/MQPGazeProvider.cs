using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GazeBimanual.Utils;
using UnityEngine;

namespace GazeBimanual
{
    public class MQPGazeProvider : MonoBehaviour
    {
        [SerializeField, Tooltip("The LayerMasks that are used to determine the GazeTarget when raycasting.")]
        public LayerMask RaycastLayerMasks = Physics.DefaultRaycastLayers;

        private OVREyeGaze leftEye;
        private OVREyeGaze rightEye;

        private Vector3 gazeOrigin;
        public Vector3 GazeOrigin => gazeOrigin;

        private Vector3 gazeDirection;
        public Vector3 GazeDirection => gazeDirection;

        public Ray GazeRay => new Ray(gazeOrigin, gazeDirection);

        [SerializeField]
        private float rayRadius = 0.02f;
        public bool IgnoreParentRigidbody = true;

        private bool didGazeRayHit;
        public bool DidGazeRayHit => didGazeRayHit;

        private RaycastHit hitInfo;
        public RaycastHit HitInfo => hitInfo;

        private RaycastHit[] raycastHits;
        public RaycastHit[] RaycastHits => raycastHits;
        [SerializeField]

        private GameObject gazeTarget;
        public GameObject GazeTarget => gazeTarget;

        public LineRayIndicator lineRayIndicator;

        // Start is called before the first frame update
        void Start()
        {
            bool successfulGazeSetup = InitializeGazeSetup();
            if (!successfulGazeSetup)
            {
                Debug.LogError("MQPGazeProvider failed to initialize."
                + "Please ensure that the OVREyeGaze components are present in the scene.\n"
                + "Missing OVREyeGaze components in the scene."
                + "You can use the Meta SDK Building Blocks to add Eye Gaze.");
                this.enabled = false;
                return;
            }
        }

        // Update is called once per frame
        void Update()
        {
            UpdateGaze();
            UpdateRaycast();

            // If the gaze ray hits something, set the end point of the line ray to the hit point
            // Otherwise, set the end point of the line to be 10 meters away from the gaze origin
            Vector3 rayEndPoint = DidGazeRayHit ? HitInfo.point : GazeOrigin + GazeDirection * 10.0f;
            // Vector3 rayEndPoint = GazeOrigin + GazeDirection * 10.0f;
            // If lineRayIndicator is not null, then update the end position of the line ray
            lineRayIndicator?.UpdatePosition(GazeRay, rayEndPoint);
        }

        /// <summary>
        /// Update the combined gaze origin and direction based on the left and 
        /// right eye positions and directions
        /// </summary>
        private void UpdateGaze()
        {
            gazeOrigin = (leftEye.transform.position + rightEye.transform.position) / 2.0f;
            gazeDirection = ((leftEye.transform.forward + rightEye.transform.forward) / 2.0f).normalized;
        }

        /// <summary>
        /// Update the gaze raycast and the gaze target based on the combined gaze ray
        /// </summary>
        private void UpdateRaycast()
        {
            // didGazeRayHit = Physics.Raycast(GazeRay, out hitInfo, Mathf.Infinity, RaycastLayerMasks);

            raycastHits = Physics.RaycastAll(GazeRay, Mathf.Infinity, RaycastLayerMasks);
            didGazeRayHit = raycastHits.Length > 0;

            // Sort the raycast hits by distance
            raycastHits = raycastHits.OrderBy(h => h.distance).ToArray();
            // Get the game objects that were hit by the raycast

            // DidGazeRayHit = ConeCastExtension.ConeCast(GazeOrigin, rayRadius, GazeDirection, out hitInfo, Mathf.Infinity, 30.0f);
            // DidGazeRayHit = Physics.SphereCast(GazeRay, rayRadius, out hitInfo, Mathf.Infinity, raycastLayerMasks);
            if (didGazeRayHit == false)
            {
                hitInfo = default;
                gazeTarget = null;
                return;
            }
            
            hitInfo = raycastHits[0];
            gazeTarget = IgnoreParentRigidbody ? hitInfo.collider.gameObject : hitInfo.transform.gameObject;
        }

        /// <summary>
        /// Find the OVREyeGaze components in the scene and assign them to the 
        /// leftEye and rightEye fields. 
        /// </summary>
        /// <returns>
        /// If either of the fields are null, then return false, otherwise 
        /// return true.
        /// </returns>
        private bool InitializeGazeSetup()
        {
            OVREyeGaze[] eyeGazes = FindObjectsOfType<OVREyeGaze>();
            foreach (OVREyeGaze eyeGaze in eyeGazes)
            {
                if (eyeGaze.Eye == OVREyeGaze.EyeId.Left)
                    leftEye = eyeGaze;
                else if (eyeGaze.Eye == OVREyeGaze.EyeId.Right)
                    rightEye = eyeGaze;
            }
            if (leftEye == null || rightEye == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the distance from the gaze ray to the point
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float DistanceToLine(Ray ray, Vector3 point)
        {
            return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
        }

        public void SetGazeRayIndicatorStatus(bool status)
        {
            if (lineRayIndicator == null)
                return;
            lineRayIndicator.enabled = status;
        }
    }
}
