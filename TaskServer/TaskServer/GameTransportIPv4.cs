using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TaskServer
{
    public class GameTransportIPv4 : IGameTransport
    {
        private Socket sock;

        public GameTransportIPv4()
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Blocking = false;
        }

        public void Bind(string address, int port)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address), port);
            sock.Bind(endPoint);
        }

        

        public byte[] Recv(int bufferSize, ref EndPoint sender)
        {
            int rlen = -1;
            byte[] data = new byte[bufferSize];
            try
            {
                rlen = sock.ReceiveFrom(data, ref sender);
                if (rlen <= 0)
                    return null;
            }
            catch
            {
                return null;
            }
            byte[] trueData = new byte[rlen];
            Buffer.BlockCopy(data, 0, trueData, 0, rlen);
            return trueData;
        }

        public EndPoint CreateEndPoint()
        {
            return new IPEndPoint(0, 0);
        }

        public bool Send(byte[] data, EndPoint endPoint)
        {
            bool success = false;
            try
            {
                int rlen = sock.SendTo(data, endPoint);
                if (rlen == data.Length)
                    success = true;
            }
            catch
            {
                success = false;
            }
            return success;
        }
    }
}