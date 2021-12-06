using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityServer.Common.Packet
{
    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(float rX, float rY, float rZ)
        {
            x = rX;
            y = rY;
            z = rZ;
        }

        public static implicit operator Vector3(SerializableVector3 rValue)
        {
            return new Vector3(rValue.x, rValue.y, rValue.z);
        }

        public static implicit operator SerializableVector3(Vector3 rValue)
        {
            return new SerializableVector3(rValue.x, rValue.y, rValue.z);
        }
    }

    [Serializable]
    public struct SerializableQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public SerializableQuaternion(float rX, float rY, float rZ, float rW)
        {
            x = rX;
            y = rY;
            z = rZ;
            w = rW;
        }

        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
        }

        public static implicit operator Quaternion(SerializableQuaternion rValue)
        {
            return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }

        public static implicit operator SerializableQuaternion(Quaternion rValue)
        {
            return new SerializableQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }
    }

    [Serializable]
    public class MsgCliSvr_Greeting_Req
    {
        public const uint MSG_ID = 00000001;
        public string text;
    }

    [Serializable]
    public class MsgSvrCli_Greeting_Ans
    {
        public const uint MSG_ID = 00000001;
        public string text;
    }

    [Serializable]
    public class MsgCliSvr_Greeting_Ntf
    {
        public const uint MSG_ID = 00000002;
        public string text;
    }

    [Serializable]
    public class MsgCliSvr_CreateRoom_Req
    {
        public const uint MSG_ID = 00000003;
    }

    [Serializable]
    public class MsgSvrCli_CreateRoom_Ans
    {
        public const uint MSG_ID = 00000003;
    }

    [Serializable]
    public class ObjectTransform
    {
        public uint id;
        public SerializableVector3 localPosition;
        public SerializableVector3 velocity;
        public SerializableQuaternion rotation;
    }

    [Serializable]
    public class MsgSvrCli_CreateSphere_Ntf
    {
        public const uint MSG_ID = 00000004;
        public uint id;
        public SerializableVector3 localPosition;
        public SerializableVector3 velocity;
        public SerializableQuaternion rotation;
    }

    [Serializable]
    public class MsgSvrCli_SyncPosition_Ntf
    {
        public const uint MSG_ID = 00000005;
        public List<ObjectTransform> transforms;
    }

    [Serializable]
    public class MsgCliSvr_HitSphere_Ntf
    {
        public const uint MSG_ID = 00000006;
        public uint id;
        public SerializableVector3 hitDirection;
    }
}