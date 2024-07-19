using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GazeBimanual.CDGain
{
    public class TestMovementCDGain : MonoBehaviour
    {
        [SerializeField]
        private float speed;
        [SerializeField]
        private Vector3 direction;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            this.transform.position = this.transform.position + (direction.normalized * speed * Time.deltaTime);
        }
    }
}
