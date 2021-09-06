using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    Socket socket;

    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7000);

        socket.Bind(endPoint);
        socket.Listen(4000);

        socket.BeginAccept(AcceptCallback, null);
    }

    void AcceptCallback(IAsyncResult ar)
    {
        Socket client = socket.EndAccept(ar);
        Debug.Log("accept");
        socket.BeginAccept(AcceptCallback, null);
    }
}
