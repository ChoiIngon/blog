using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityServer.Client
{
    public class Sphere : MonoBehaviour
    {
        public uint id;
        public Rigidbody rigidBody;

        void Start()
        {
            rigidBody = GetComponent<Rigidbody>();
        }
    }
}