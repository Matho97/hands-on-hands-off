using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static OVRHand;

namespace GazeBimanual.Utils
{
    public class OVRHandPinchEventWrapper : MonoBehaviour
    {
        [SerializeField]
        private OVRHand ovrHand;
        [SerializeField]
        private HandFinger finger;

        private bool wasPinching;

        public UnityEvent OnPinchStart;
        public UnityEvent WhilePinching;
        public UnityEvent OnPinchEnd;

        void Start()
        {
            if (ovrHand == null)
            {
                Debug.LogError("OVRHand is not assigned to GazePinchController");
                this.enabled = false;
                return;
            }
        }

        void Update()
        {
            bool isPinching = ovrHand.GetFingerIsPinching(finger);

            if (isPinching && !wasPinching)
            {
                OnPinchStart?.Invoke();
                wasPinching = true;
            }
            else if (!isPinching && wasPinching)
            {
                OnPinchEnd?.Invoke();
                wasPinching = false;
            }

            if (!isPinching)
                return;

            WhilePinching?.Invoke();
        }
    }
}
