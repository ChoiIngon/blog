using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Packet
{
    [Serializable]
    public enum ErrorCode
    {
        Success
    }

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
    public class Object
    {
        public uint id;
        public SerializableVector3 localPosition;
        public SerializableVector3 velocity;
        public SerializableQuaternion rotation;
    }

    [Serializable]
    public class Ball : Object
    {
    }

    [Serializable]
    public class Bar : Object
    {
    }

    [Serializable]
    public class MsgCliSvr_Join_Req
    {
        public const uint PACKET_ID = 00000001;
        public uint roomId;
    }

    [Serializable]
    public class MsgSvrCli_Join_Ans
    {
        public const uint PACKET_ID = 00000001;
        public ErrorCode errorCode;
    }

    [Serializable]
    public class MsgSvrCli_SyncBlock_Ntf
    {
        public const uint PACKET_ID = 00000003;
        public uint id;
        public uint durability;
    }

    [Serializable]
    public class Player
    {
        public int playerNum;
        public Ball ball = new Ball();
        public Bar bar = new Bar();
    }
    [Serializable]
    public class MsgSvrCli_Ready_Ntf
    {
        public const uint PACKET_ID = 00000005;
        public List<Block> blocks = new List<Block>();
        public List<Player> players = new List<Player>();
        public int playerNum;
    }

    [Serializable]
    public class MsgCliSvr_SyncBar_Ntf
    {
        public const uint PACKET_ID = 00000006;
        public SerializableVector3 destination;
    }

    [Serializable]
    public class MsgSvrCli_SyncBar_Ntf
    {
        public const uint PACKET_ID = 00000006;
        public uint objectId;
        public SerializableVector3 destination;
    }

    [Serializable]
    public class MsgSvrCli_SyncBall_Ntf
    {
        public const uint PACKET_ID = 00000007;
        public Ball ball = new Ball();
    }

    [Serializable]
    public class MsgCliSvr_Start_Ntf
    {
        public const uint PACKET_ID = 00000008;
    }

    [Serializable]
    public class MsgSvrCli_Start_Ntf
    {
        public const uint PACKET_ID = 00000008;
        public uint objectId;
    }

    [Serializable]
    public class MsgSvrCli_DestroyObject_Ntf
    {
        public const uint PACKET_ID = 00000009;
        public List<uint> objectIds = new List<uint>();
    }

}