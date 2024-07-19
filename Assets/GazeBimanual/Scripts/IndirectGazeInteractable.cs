using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.Events;

namespace GazeBimanual
{
    public class IndirectGazeInteractable : MonoBehaviour
    {
        public enum InteractionType
        {
            None,
            Pulling,
            Holding,
            Both
        }

        [Header("Gaze")]

        private bool isGazeOn = false;
        public bool IsGazeOn => isGazeOn;
        public Color gazeOnColor = Color.yellow;
        public Color interactionColor = new Color(0.2f, 0.2f, 0.2f, 0.75f);
        public Color defaultColor = Color.white;

        public UnityEvent OnGazeStart;
        public UnityEvent OnGazeEnd;

        [Header("Interaction")]
        [SerializeField]
        private InteractionType interaction = InteractionType.Both;
        public InteractionType Interaction { get => interaction; set => interaction = value; }

        [SerializeField]
        private bool isBeingInteracting = false;
        public bool IsBeingInteracted => isBeingInteracting;

        public UnityEvent OnInteractStart;
        public UnityEvent OnInteractEnd;

        [SerializeField]
        private Transform targetTransformOverride;
        public Transform TargetTransform => targetTransformOverride != null ? targetTransformOverride : this.transform; // If targetInteractable is null, return this transform

        [SerializeField]
        private HandGrabInteractable ovrInteractable;

        [SerializeField]
        private Outline targetOutline;

        [SerializeField]
        private GazePinchController gazePinchController;
        public GazePinchController GazePinchController => gazePinchController;

        void Start()
        {
            if (targetOutline == null && targetTransformOverride != null)
                targetOutline = targetTransformOverride?.GetComponent<Outline>();
        }

        void Update()
        {

        }

        public void GazeState(bool state)
        {
            if (this.isActiveAndEnabled == false)
                return;

            if (isGazeOn == false && state)
                OnGazeStart?.Invoke();
            else if (isGazeOn && state == false)
                OnGazeEnd?.Invoke();

            isGazeOn = state;

            if (targetOutline == null || isBeingInteracting)
                return;
            targetOutline.enabled = state;
            targetOutline.OutlineColor = state ? gazeOnColor : defaultColor;
        }

        public void InteractionState(bool state, Color color = default, GazePinchController gazePinchController = null)
        {
            if (this.isActiveAndEnabled == false)
                return;

            if (isBeingInteracting == false && state)
                OnInteractStart?.Invoke();
            else if (isBeingInteracting && state == false)
                OnInteractEnd?.Invoke();

            isBeingInteracting = state;
            if (isBeingInteracting && gazePinchController != null)
                this.gazePinchController = gazePinchController;
            else if (isBeingInteracting == false)
                this.gazePinchController = null;

            if (ovrInteractable != null)
                ovrInteractable.enabled = !state;

            if (targetOutline == null)
                return;

            targetOutline.enabled = state || isGazeOn;

            // Check if the color has been overriden, if not, use the interactionColor
            Color stateColor = color != default ? color : interactionColor;
            // get the color to set in case interaction is off but gaze is on
            Color gazeFallBackColor = isGazeOn ? gazeOnColor : defaultColor;
            // Set the color based on the state
            targetOutline.OutlineColor = state ? stateColor : gazeFallBackColor;
        }

        void Awake()
        {
            if (targetOutline == null)
                targetOutline = GetComponent<Outline>();

            if (ovrInteractable == null)
                ovrInteractable = GetComponent<HandGrabInteractable>();
        }

        void OnDisable()
        {
            isGazeOn = false;
            isBeingInteracting = false;
            this.gazePinchController?.ForceRelease();
        }
    }
}
