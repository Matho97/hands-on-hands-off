using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GazeBimanual
{
    public class GazeBimanualInteraction : MonoBehaviour
    {
        [Header("Gaze")]
        [SerializeField]
        private MQPGazeProvider mQPGazeProvider;
        public MQPGazeProvider MQPGazeProvider => mQPGazeProvider;

        [SerializeField]
        private GazePinchController leftGazePinchController;
        public GazePinchController LeftGazePinchController => leftGazePinchController;
        [SerializeField]
        private GazePinchController rightGazePinchController;
        public GazePinchController RightGazePinchController => rightGazePinchController;

        [SerializeField]
        private Color gazeOnColor;

        private IndirectGazeInteractable gazeObject;
        public IndirectGazeInteractable GazeObject => gazeObject;

        private bool gazeOnObject;
        public bool GazeOnObject => gazeOnObject;

        // If (1) the gaze object is being interacted with, 
        // (2) the gaze object is not null,
        // (3) the gaze object's interaction type is not "None"
        // (4) the gaze object is being interacted with
        public bool IsGazeObjectBeingInteractedWith => GazeOnObject && GazeObject != null && GazeObject.Interaction != IndirectGazeInteractable.InteractionType.None && GazeObject.IsBeingInteracted;

        void Start()
        {
            // Ensure that required components are set
            if (EnsureComponentsSet() == false)
            {
                this.enabled = false;
                return;
            }
        }

        void Update()
        {
            // if (gazeObject != null && gazeObject.IsBeingInteracted) // If the object is being interacted, don't change the gaze state
            //     return;

            // If the gaze provider has a target, get the first interactable components in the heirarchy of the target
            if (mQPGazeProvider.GazeTarget == null)
            {
                UpdateGazeState(null);
                return;
            }

            // IndirectGazeInteractable newGazeObject = mQPGazeProvider.GazeTarget?.GetComponentInParent<IndirectGazeInteractable>();
            IndirectGazeInteractable newGazeObject = mQPGazeProvider.GazeTarget?.GetComponent<IndirectGazeInteractable>();
            if (newGazeObject?.isActiveAndEnabled == false)
                newGazeObject = null;
            UpdateGazeState(newGazeObject);
        }

        private void UpdateGazeState(IndirectGazeInteractable newGazeObject)
        {
            // If the left hand is not currently pinching (interacting) and the left hand is in reach of the new gaze object
            bool isLeftHandTooClose = leftGazePinchController != null && leftGazePinchController.IsPinching == false && leftGazePinchController.IsInteractableInFingerReach(newGazeObject);

            // If the right hand is not currently pinching (interacting) and the right hand is in reach of the new gaze object
            bool isRightHandTooClose = rightGazePinchController != null && rightGazePinchController.IsPinching == false && rightGazePinchController.IsInteractableInFingerReach(newGazeObject);

            // Abort if the new gaze object is null, the new gaze object's interaction type is "None", or either hand is too close to the new gaze object
            // and set gaze object to null
            if (newGazeObject == null
             || newGazeObject.Interaction == IndirectGazeInteractable.InteractionType.None
             || isLeftHandTooClose
             || isRightHandTooClose)
            {
                gazeOnObject = false;
                if (gazeObject != null)
                    gazeObject.GazeState(false);
                gazeObject = null;
                return;
            }

            // If the gaze object has changed, set the previous gaze object's gaze state to false
            if (gazeObject != newGazeObject)
            {
                gazeOnObject = false;
                gazeObject?.GazeState(false);
            }
            // Set the new gaze object as the gaze object
            gazeObject = newGazeObject;

            // If gaze was not on the object before, set the gaze state to true
            if (!gazeOnObject || gazeObject.IsGazeOn == false)
            {
                gazeObject.GazeState(true);
                gazeOnObject = true;
            }
        }

        private bool EnsureComponentsSet()
        {
            // If the gaze provider is not set, try to get it from the current game object
            if (mQPGazeProvider == null)
            {
                mQPGazeProvider = this.GetComponent<MQPGazeProvider>();
                if (mQPGazeProvider == null)
                {
                    Debug.LogError("MQPGazeProvider is not assigned to GazeBimanualInteraction");
                    return false;
                }
            }

            // If the left or right gaze pinch controllers are not set, try to get them from the current game object
            /* if (leftGazePinchController != null && rightGazePinchController != null)
                return true;

            GazePinchController[] gazePinchControllers = this.GetComponentsInChildren<GazePinchController>();
            if (leftGazePinchController == null)
            {
                leftGazePinchController = gazePinchControllers.Where(gpc => gpc.Handedness == GazePinchController.HandType.Left).FirstOrDefault();
                if (leftGazePinchController == null)
                {
                    Debug.LogError("LeftGazePinchController is not assigned to GazeBimanualInteraction");
                    return false;
                }
            }

            if (rightGazePinchController == null)
            {
                rightGazePinchController = gazePinchControllers.Where(gpc => gpc.Handedness == GazePinchController.HandType.Right).FirstOrDefault();
                if (rightGazePinchController == null)
                {
                    Debug.LogError("RightGazePinchController is not assigned to GazeBimanualInteraction");
                    return false;
                }
            } */

            return true;
        }

        void OnDisable()
        {
            if (gazeObject != null)
                gazeObject.GazeState(false);
            gazeObject = null;
            gazeOnObject = false;
        }
    }
}
