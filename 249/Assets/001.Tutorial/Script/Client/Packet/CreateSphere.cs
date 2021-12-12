using System;
using UnityEngine;
using UnityServer.Common.Packet;

namespace UnityServer.Client.Packet
{
    public static class CreateSphere
    {
        public static void OnReceive(MsgSvrCli_CreateSphere_Ntf ntf)
        {
            GameObject go = UnityEngine.Object.Instantiate<GameObject>(Main.Instance.spherePrefab);
            Sphere sphere = go.AddComponent<Sphere>();
            sphere.gameObject.layer = LayerMask.NameToLayer("Client");
            sphere.rigidBody = sphere.GetComponent<Rigidbody>();
            sphere.id = ntf.id;
            sphere.transform.localPosition = ntf.localPosition;
            sphere.transform.rotation = ntf.rotation;
            sphere.rigidBody.velocity = ntf.velocity;

            Transform sphereTransform = Main.Instance.transform.Find("Room/Spheres");
            sphere.transform.SetParent(sphereTransform, false);

            Main.Instance.spheres.Add(sphere.id, sphere);
        }
    }
}
