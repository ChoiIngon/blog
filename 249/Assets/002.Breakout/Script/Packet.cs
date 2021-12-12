using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Packet
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
    public struct Block
    {
        public uint id;
        public uint type;
        public SerializableVector3 localPosition;
    }

    [Serializable]
    public struct MsgCliSvr_Join_Req
    {
        public const uint PACKET_ID = 00000001;
        public uint roomId;
    }

    [Serializable]
    public struct MsgSvrCli_Join_Ans
    {
        public const uint PACKET_ID = 00000001;
        public uint errorCode;
        List<Block> blocks;
    }

    [Serializable]
    public struct MsgCliSvr_BallTransform_Ntf
    {
        public const uint PACKET_ID = 00000002;
        public SerializableVector3 localPosition;
        public SerializableVector3 velocity;
        public SerializableQuaternion rotation;
    }

    [Serializable]
    public struct MsgSvrCli_BallTransform_Ntf
    {
        public const uint PACKET_ID = 00000002;
        public uint id;
        public SerializableVector3 localPosition;
        public SerializableVector3 velocity;
        public SerializableQuaternion rotation;
    }

    [Serializable]
    public struct MsgSvrCli_BlockHit_Ntf
    {
        public const uint PACKET_ID = 00000003;
        public uint id;
        public uint durability;
    }
}