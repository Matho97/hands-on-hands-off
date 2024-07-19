using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction.Input;
using static GazeBimanual.IndirectGazeInteractable;
using GazeBimanual.Utils;
using Oculus.Interaction;
using System;

namespace GazeBimanual
{
    [ExecuteAlways]
    public class GazePinchController : MonoBehaviour
    {
        [Header("Required Components")]
        [SerializeField]
        private GazeBimanualInteraction gazeBimanualController;
        public GazeBimanualInteraction GazeBimanualController => gazeBimanualController;

        public enum HandType
        {
            Left,
            Right
        }
        public HandType Handedness;
        public HandJointId FingerPositionJoint = HandJointId.HandThumbTip;
        public OVRHand.HandFinger FingerPinchJoint = OVRHand.HandFinger.Index;

        [SerializeField]
        private OVRHand ovrHand;
        public OVRHand OvrHand => ovrHand;
        [SerializeField]
        private Hand oculusHand;
        public Hand OculusHand => oculusHand;

        [Tooltip("The interactables this interactor can or can't use. Is determined by comparing this interactor's TagSetFilter component(s) to the TagSet component on the interactables.")]
        [SerializeField]
        private List<TagSetFilter> interactableFilters = new List<TagSetFilter>();

        [Header("Gaze Pinch Parameters")]

        [SerializeField, Tooltip("The object that the user is currently pinching (if any) (Set automatically)")]
        private IndirectGazeInteractable gazePinchObject;
        public IndirectGazeInteractable GazePinchObject => gazePinchObject;

        [SerializeField]
        private InteractionType pinchInteractionType = InteractionType.Both;
        public InteractionType PinchInteractionType { get => pinchInteractionType; set => pinchInteractionType = value; }

        public bool IsInteracting => gazePinchObject != null && isPinching;

        [SerializeField]
        private float minPinchStrength = 0.5f;

        private bool isPinching = false;
        public bool IsPinching => isPinching;
        private bool wasPinching = false;

        public UnityEvent<GazePinchController> OnPinchStart;
        public UnityEvent<GazePinchController> WhilePinching;
        public UnityEvent<GazePinchController> OnPinchEnd;

        [Header("Finger Parameters")]

        [SerializeField]
        private GameObject fingerTipObject;
        public GameObject FingerTipObject => fingerTipObject;
        [SerializeField]
        private Transform rotationIntermediary;
        public Transform RotationIntermediary => rotationIntermediary;
        public Vector3 FingerTipPosition => fingerTipObject.transform.position;
        public Quaternion FingerTipRotation => fingerTipObject.transform.rotation;

        [SerializeField]
        private bool isFingerCloseToObject = false;
        [SerializeField]
        private float handMinDistanceThreshold = 0.1f;
        public float HandMinDistanceThreshold => handMinDistanceThreshold;
        [SerializeField]
        private float handMaxDistanceThreshold = 0.5f;
        public float HandMaxDistanceThreshold => handMaxDistanceThreshold;
        [SerializeField]
        private float manipulationMultiplier = 1f;

        private Vector3 gazePinchObjectStartPosition;
        private Vector3 pinchStartPosition;

        private OneEuroFilter<Vector3> fingerTipPositionFilter;
        private Vector3 filteredFingerTipPosition;
        private OneEuroFilter<Quaternion> rotationIntermediaryFilter;
        private Quaternion filteredRotationIntermediary;

        private float startTwoPinchDistance = 0f;
        private Vector3 gazePinchObjectStartScale;

        private List<IndirectGazeInteractable> interactables;

        [Header("€1 Filter Parameters")]
        public bool filterOn = true;

        public float filterFrequency = 90.0f;
        public float filterMinCutoff = 0.5f;
        public float filterBeta = 2.0f;
        public float filterDcutoff = 1.0f;

        [Header("CD Gain Parameters")]
        [SerializeField]
        private bool applyCDGain = false;
        public bool ApplyCDGain { get => applyCDGain; set => applyCDGain = value; }

        public AnimationCurve cdGainCurve = AnimationCurve.Constant(0.0f, 1.0f, 40.0f);
        public float cdGainMultiplier = 1.0f;

        private float rawSpeed;
        private float cdGain;
        private float cdGainSpeed;

        private Vector3 previousFingerPosition;

        void Awake()
        {
            AutoWireComponents();
        }

        void Start()
        {
            AutoWireComponents();
            // Ensure that required components are assigned
            if (gazeBimanualController == null)
            {
                Debug.LogError("GazeBimanualController is not assigned to GazePinchController");
                this.enabled = false;
                return;
            }
            if (ovrHand == null)
            {
                Debug.LogError("OVRHand is not assigned to GazePinchController");
                this.enabled = false;
                return;
            }
            if (oculusHand == null)
            {
                Debug.LogError("OculusHand is not assigned to GazePinchController");
                this.enabled = false;
                return;
            }
            if (fingerTipObject == null)
            {
                Debug.LogError("FingerTipObject is not assigned to GazePinchController");
                this.enabled = false;
                return;
            }
            if (rotationIntermediary == null)
            {
                Debug.LogError("RotationIntermediary is not assigned to GazePinchController");
                this.enabled = false;
                return;
            }

            interactables = FindObjectsOfType<IndirectGazeInteractable>().ToList();

            fingerTipPositionFilter = new OneEuroFilter<Vector3>(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
            rotationIntermediaryFilter = new OneEuroFilter<Quaternion>(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
        }

        void Update()
        {
            if (Application.isPlaying == false)
                return;

            // Update 1€ filter parameters in runtime if the filter is on
            if (filterOn)
            {
                fingerTipPositionFilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
                rotationIntermediaryFilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
            }

            UpdateFingerTipPosition(FingerPositionJoint);

            isPinching = GetIsPinching(ovrHand, FingerPinchJoint);
            HandlePinching(pinchInteractionType);
        }

        private void HandlePinching(InteractionType interaction)
        {
            // If the user is pinching, and the user was not pinching in the previous frame, handle the pinch start
            if (isPinching && !wasPinching)
            {
                bool gazePinchStarted = HandleGazePinchStart(interaction);
                if (gazePinchStarted)
                {
                    OnPinchStart?.Invoke(this);
                    previousFingerPosition = fingerTipObject.transform.position;
                }
                wasPinching = true;
            }
            else if (!isPinching && wasPinching) // If the user is not pinching, and the user was pinching in the previous frame, handle the pinch end
            {
                OnPinchEnd?.Invoke(this);
                wasPinching = false;
                gazePinchObject?.InteractionState(false);
                gazePinchObject = null;
            }

            if (!isPinching || !gazePinchObject)
                return;

            // If the user is pinching, and the user is pinching the gaze object, handle the pinch
            WhilePinching?.Invoke(this);

            filteredFingerTipPosition = fingerTipPositionFilter.Filter(fingerTipObject.transform.position);
            Vector3 newFingerPosition = filterOn ? filteredFingerTipPosition : fingerTipObject.transform.position;

            // If we should apply CD gain
            if (applyCDGain)
            {
                // Calculate the speed of the user's finger
                Vector3 movementDirection = newFingerPosition - previousFingerPosition;
                rawSpeed = movementDirection.magnitude / Time.deltaTime;

                // Calculate the CD gain
                cdGainSpeed = GetCDGainSpeed(cdGainCurve, cdGainMultiplier, rawSpeed);

                // Move the gaze object according to the CD gain
                gazePinchObject.TargetTransform.transform.position += movementDirection.normalized * cdGainSpeed;

                previousFingerPosition = newFingerPosition;
            }
            else
            {
                // Otherwise, move the target according to the user's finger movement
                gazePinchObject.TargetTransform.transform.position = gazePinchObjectStartPosition + (newFingerPosition - pinchStartPosition) * manipulationMultiplier;
            }

            // Update the rotation of the gaze object
            filteredRotationIntermediary = rotationIntermediaryFilter.Filter(rotationIntermediary.rotation);
            Quaternion newRotationIntermediary = filterOn ? filteredRotationIntermediary : rotationIntermediary.rotation;
            gazePinchObject.TargetTransform.transform.rotation = newRotationIntermediary;
        }

        private float GetCDGain(AnimationCurve cdGainCurve, float cdGainMultiplier, float speed)
        {
            return cdGainCurve.Evaluate(speed) * cdGainMultiplier;
        }

        private float GetCDGainSpeed(AnimationCurve cdGainCurve, float cdGainMultiplier, float speed)
        {
            return speed * GetCDGain(cdGainCurve, cdGainMultiplier, speed);
        }

        private bool HandleGazePinchStart(InteractionType interaction)
        {
            if (gazeBimanualController.GazeObject == null || gazeBimanualController.isActiveAndEnabled == false)
                return false;

            // Don't allow indirect interaction if the user's finger is too close to an interactable object
            // bool isHandTooClose = IsInteractableInFingerReach();
            // if (isHandTooClose)
            //     return false;

            // Check if the interaction type of the gaze object matches the interaction type of the pinch controller
            bool either = gazeBimanualController.GazeObject.Interaction == InteractionType.Pulling || gazeBimanualController.GazeObject.Interaction == InteractionType.Holding || gazeBimanualController.GazeObject.Interaction == InteractionType.Both;
            bool specific = gazeBimanualController.GazeObject.Interaction == interaction && gazeBimanualController.GazeObject.Interaction != InteractionType.None;
            bool interactionMatches = interaction == InteractionType.Both ? either : specific;
            if (gazeBimanualController.IsGazeObjectBeingInteractedWith || interactionMatches == false)
                return false;

            if (CanSelect(gazeBimanualController.GazeObject) == false)
                return false;

            // Set the pinch object to the gaze object
            gazePinchObject = gazeBimanualController.GazeObject;
            // Set the gaze object's interaction state to true if possible
            gazePinchObject?.InteractionState(true, gazePinchController: this);

            // Get the start position and rotation of the gaze object
            gazePinchObjectStartPosition = gazePinchObject.TargetTransform.transform.position;
            rotationIntermediary.rotation = gazePinchObject.TargetTransform.transform.rotation;

            // Reset the 1€ filters
            fingerTipPositionFilter = new OneEuroFilter<Vector3>(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
            rotationIntermediaryFilter = new OneEuroFilter<Quaternion>(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);

            // Get the start position of the user's finger (filtered if the filter is on)
            filteredFingerTipPosition = fingerTipPositionFilter.Filter(fingerTipObject.transform.position);
            pinchStartPosition = filterOn ? filteredFingerTipPosition : fingerTipObject.transform.position;

            return true;

        }

        private bool GetIsPinching(OVRHand hand, OVRHand.HandFinger finger)
        {
            if (hand == null)
                return false;

            // Check if the user is pinching with the index finger using the OVRHand API
            bool isIndexFingerPinching = hand.GetFingerIsPinching(finger);

            float pinchStrength = hand.GetFingerPinchStrength(finger);
            OVRHand.TrackingConfidence pinchConfidence = hand.GetFingerConfidence(finger);
            if (!isIndexFingerPinching && isPinching && pinchStrength > minPinchStrength)
            {
                // Debug.Log($"{handType} pinch strength: {pinchStrength}, confidence: {pinchConfidence}");
                isIndexFingerPinching = true;
            }
            return isIndexFingerPinching;
        }

        public bool GetHandJointPose(Hand oculusHand, HandJointId jointId, out Pose handJointPose)
        {
            handJointPose = Pose.identity;
            if (oculusHand == null)
                return false;

            // Get the position and rotation of the user's thumb finger tip using the Oculus Hand API
            // Using thumb tip as it is more stable than the index finger tip
            bool successfulPose = oculusHand.GetJointPose(jointId, out handJointPose);
            return successfulPose;
        }

        public bool GetHandJointPose(HandJointId jointId, out Pose handJointPose)
        {
            handJointPose = Pose.identity;
            if (oculusHand == null)
                return false;

            bool successfulPose = GetHandJointPose(oculusHand, jointId, out handJointPose);
            return successfulPose;
        }

        public bool GetFingerTipPosition(out Pose fingerTipPose)
        {
            fingerTipPose = Pose.identity;
            if (oculusHand == null)
                return false;

            // Get the position and rotation of the user's thumb finger tip using the Oculus Hand API
            // Using thumb tip as it is more stable than the index finger tip
            bool success = GetHandJointPose(oculusHand, FingerPositionJoint, out fingerTipPose);
            return success;
        }

        private void UpdateFingerTipPosition(HandJointId jointId)
        {
            if (oculusHand == null)
                return;

            // Get the position and rotation of the user's thumb finger tip using the Oculus Hand API
            // Using thumb tip as it is more stable than the index finger tip
            bool success = GetHandJointPose(oculusHand, jointId, out Pose fingerTipPose);
            if (success == false)
                return;

            fingerTipObject.transform.position = fingerTipPose.position;
            fingerTipObject.transform.rotation = fingerTipPose.rotation;

            // filteredFingerTipPosition = fingerTipPositionFilter.Filter(fingerTipObject.transform.position);
            // filteredRotationIntermediary = rotationIntermediaryFilter.Filter(rotationIntermediary.rotation);
        }

        private bool IsInteractableInHandReach(Vector3 jointPosition)
        {
            return interactables.Any(x => x != null && Vector3.Distance(x.transform.position, jointPosition) < handMinDistanceThreshold);
        }

        public bool IsInteractableInFingerReach()
        {
            return interactables.Any(x => x != null && Vector3.Distance(x.transform.position, FingerTipPosition) < handMinDistanceThreshold);
        }

        public bool IsInteractableInFingerReach(IndirectGazeInteractable interactable)
        {
            return interactable != null && Vector3.Distance(interactable.transform.position, FingerTipPosition) < handMinDistanceThreshold;
        }

        public bool CanSelect(IndirectGazeInteractable interactable)
        {
            if (interactableFilters == null)
                return true;

            foreach (IGameObjectFilter interactableFilter in interactableFilters)
            {
                if (!interactableFilter.Filter(interactable.gameObject))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Force the release of the object that the user is pinching and reset the state of the pinch controller
        /// </summary>
        /// <returns>True if the user was pinching an object and the object was released, false otherwise</returns>
        public bool ForceRelease()
        {
            if (gazePinchObject == null)
                return false;

            isPinching = false;
            OnPinchEnd?.Invoke(this);
            gazePinchObject?.InteractionState(false);
            gazePinchObject = null;
            return true;
        }

        void OnDisable()
        {
            ForceRelease();
        }

        private void AutoWireComponents()
        {
            if (gazeBimanualController == null)
            {
                gazeBimanualController = FindObjectOfType<GazeBimanualInteraction>();
                if (gazeBimanualController != null)
                    Debug.Log($"Auto-wiring succeeded on {this.name}: GazeBimanualInteraction was linked to {gazeBimanualController.name}");
            }

            if (ovrHand == null)
            {
                OVRHand[] hands = FindObjectsOfType<OVRHand>();
                foreach (OVRHand hand in hands)
                {
                    if (hand.name.ToLower().Contains(Handedness.ToString().ToLower()))
                    {
                        ovrHand = hand;
                        Debug.Log($"Auto-wiring succeeded on {this.name}: OvrHand was linked to {ovrHand.name}");
                    }
                }
            }

            if (oculusHand == null)
            {
                SyntheticHand[] oculusHands = FindObjectsOfType<SyntheticHand>();
                foreach (SyntheticHand hand in oculusHands)
                {
                    if (hand.name.ToLower().Contains(Handedness.ToString().ToLower()))
                    {
                        oculusHand = hand;
                        Debug.Log($"Auto-wiring succeeded on {this.name}: OculusHand was linked to {oculusHand.name}");
                    }
                }
            }

            if (fingerTipObject == null)
            {
                fingerTipObject = this.transform.GetChild(0)?.gameObject;
                if (fingerTipObject != null)
                {
                    Debug.Log($"Auto-wiring succeeded on {this.name}: FingerTipObject was linked to {fingerTipObject.name}");
                }
                else
                {
                    fingerTipObject = new GameObject("FingerTip");
                    fingerTipObject.transform.parent = this.transform;
                    fingerTipObject.transform.localPosition = Vector3.zero;
                    fingerTipObject.transform.localScale = Vector3.one * 0.025f;
                    GetComponent<Collider>().enabled = false;
                }
            }

            if (rotationIntermediary == null)
            {
                rotationIntermediary = fingerTipObject.transform.GetChild(0)?.transform;
                if (rotationIntermediary != null)
                {
                    Debug.Log($"Auto-wiring succeeded on {this.name}: RotationIntermediary was linked to {rotationIntermediary.name}");
                }
                else
                {
                    rotationIntermediary = new GameObject("RotationIntermediary").transform;
                    rotationIntermediary.parent = fingerTipObject.transform;
                    rotationIntermediary.localPosition = Vector3.zero;
                    GetComponent<Collider>().enabled = false;
                }
            }
        }
    }
}
