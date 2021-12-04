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
            Common.Sphere sphere = go.AddComponent<Common.Sphere>();
            sphere.gameObject.tag = "Client";
            sphere.gameObject.layer = LayerMask.NameToLayer("Client");
            sphere.rigidBody = sphere.GetComponent<Rigidbody>();
            sphere.id = ntf.id;
            sphere.transform.localPosition = new Vector3(ntf.positionX, ntf.positionY, ntf.positionZ);
            sphere.rigidBody.velocity = new Vector3(ntf.velocityX, ntf.velocityY, ntf.velocityZ);

            Transform sphereTransform = Main.Instance.transform.Find("Room/Spheres");
            sphere.transform.SetParent(sphereTransform, false);

            Main.Instance.spheres.Add(sphere.id, sphere);
        }
    }
}
