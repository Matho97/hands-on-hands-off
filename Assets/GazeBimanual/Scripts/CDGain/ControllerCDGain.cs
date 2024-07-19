using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GazeBimanual.Utils;
using UnityEngine;

namespace GazeBimanual.CDGain
{
    public class ControllerCDGain : MonoBehaviour
    {
        [SerializeField]
        private bool applyMovement = false;
        public bool ApplyMovement { get => applyMovement; set => applyMovement = value; }

        public AnimationCurve Curve;
        public float Multiplier = 1.0f;

        private Vector3 previousPosition;
        private Vector3 movementDirection;
        [SerializeField]
        private float speed;

        public Transform target;

        private OneEuroFilter<Vector3> positionFilter;
        private Vector3 filteredPosition;

        void Start()
        {
            if (target == null)
            {
                Debug.LogError($"Target is not assigned to ControllerCDGain on object: {this.name}");
                this.enabled = false;
                return;
            }

            positionFilter = new OneEuroFilter<Vector3>(90.0f);
            filteredPosition = positionFilter.Filter(transform.position);
        }

        void Update()
        {
            filteredPosition = positionFilter.Filter(transform.position);
            movementDirection = filteredPosition - previousPosition;
            speed = movementDirection.magnitude / Time.deltaTime;
            if (ApplyMovement)
            {
                float cdGain = Curve.Evaluate(speed) * Multiplier;
                float deviceSpeed = speed * cdGain;
                Debug.Log("Speed: " + speed + " m/s" + " | CD Gain: " + cdGain + " | Device Speed " + deviceSpeed + " m/s");
                target.position = target.position + (movementDirection.normalized * deviceSpeed * Time.deltaTime);
            }

            previousPosition = filteredPosition;
        }
    }
}
