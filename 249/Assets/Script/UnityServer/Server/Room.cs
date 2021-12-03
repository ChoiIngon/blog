using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityServer.Server
{
    public class Room : MonoBehaviour
    {
        public Server.Session session;
        Transform spheres;

        void Start()
        {
            spheres = transform.Find("Spheres");
            Vector3[] initPositions = new Vector3 [] {
                new Vector3(-4, 4, -4),
                new Vector3(-3, 3, -4),
                new Vector3(-2, 2, -4),
                new Vector3(-1, 1, -4),
                new Vector3(-0, 0, -4),
                new Vector3( 1, -1, -4),
                new Vector3( 2, -2, -4),
                new Vector3( 3, -3, -4),
                new Vector3( 4, -4, -4)
            };
            for (uint i = 0; i < 9; i++)
            {
                GameObject go = Server.Main.Instance.CreateSphere();
                Sphere sphere = go.AddComponent<Sphere>();
                sphere.session = session;
                sphere.id = i+1;
                sphere.gameObject.name = $"Sphere{sphere.id}";
                sphere.transform.localPosition = initPositions[i];
                sphere.transform.SetParent(spheres, false);
            }
        }
    }
}