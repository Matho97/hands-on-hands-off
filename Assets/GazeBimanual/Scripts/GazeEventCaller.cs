using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.Events;

namespace GazeBimanual
{
    public class GazeEventCaller : MonoBehaviour
    {
        [SerializeField]
        private MQPGazeProvider gazeProvider;

        private bool isGazeOn = false;
        public bool IsGazeOn => isGazeOn;

        public UnityEvent OnGazeStart;
        public UnityEvent OnGazeEnd;

        void Start()
        {
            if (gazeProvider == null)
            {
                Debug.LogWarning($"GazeEventCaller: {this.name} does not have a GazeProvider assigned. Trying to find one in the scene.");
                gazeProvider = FindObjectOfType<MQPGazeProvider>();
            }
            if (gazeProvider == null)
            {
                Debug.LogError($"GazeEventCaller: {this.name} does not have a GazeProvider assigned.");
                this.enabled = false;
                return;
            }
        }

        void Update()
        {
            if (gazeProvider == null)
                return;
            if (gazeProvider.RaycastHits == null)
                return;

            // bool isGazeOnThis = gazeProvider.GazeTarget == this.gameObject;

            // Check if the gaze is on this object even through other colliders
            bool isGazeOnThis = false;
            foreach (RaycastHit hit in gazeProvider.RaycastHits)
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    isGazeOnThis = true;
                    break;
                }
            }
            GazeState(isGazeOnThis);
        }

        private void GazeState(bool state)
        {
            if (this.isActiveAndEnabled == false)
                return;

            if (isGazeOn == false && state)
                OnGazeStart?.Invoke();
            else if (isGazeOn && state == false)
                OnGazeEnd?.Invoke();

            isGazeOn = state;
        }
    }
}
