using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace TaskServer
{
    public class GameClient
    {
        private EndPoint endPoint;

        private Queue<Packet> sendQueue;

        private Dictionary<uint, Packet> ackTab;

        
        //GameServer non è più una classe statica, ora ogni GameClient ha un proprio server
        private GameServer server;
        public GameServer Server { get { return server; } }

        
        //MaLus è una proprietà di sola lettura, questo aumenta la sicurezza
        private uint malus;
        public uint Malus { get { return malus; } }
        private float malusTimeStamp;
        public float MalusTimeStamp { get { return malusTimeStamp; } }
        public bool CanReduceClientMalus { get { return malus > 0 && MalusTimeStamp <= Server.Now; } }

        public void IncreaseMalus(uint malusValue = 0)
        {
            if (malusValue == 0)
                malus++;
            else
                malus += malusValue;

            malusTimeStamp = Server.Now + 300f;
        }

        public void Ack(uint packetId)
        {
            if (ackTab.ContainsKey(packetId))
            {
                ackTab.Remove(packetId);
            }
            else
            {
                IncreaseMalus();
            }
        }


        public GameClient(EndPoint endPoint, GameServer server)
        {
            this.endPoint = endPoint;
            sendQueue = new Queue<Packet>();
            ackTab = new Dictionary<uint, Packet>();
            malus = 0;

            this.server = server;
        }

        public void ReduceMalus()
        {
            if (CanReduceClientMalus)
            {
                malus--;
            }
            else
                IncreaseMalus();
        }


        public void Process()
        {
            int packetsInQueue = sendQueue.Count;
            for (int i = 0; i < packetsInQueue; i++)
            {
                Packet packet = sendQueue.Dequeue();

                // controlla se il pacchetto deve essere inviato
                if (server.Now >= packet.SendAfter)
                {
                    packet.IncreaseAttempts();
                    if (server.Send(packet, endPoint))
                    {

                        // andato tutto correttamente
                        if (packet.NeedAck)
                        {
                            ackTab[packet.Id] = packet;
                        }
                    }
                    // in caso di errore, riprovare l'invio solo se non e' OneShot (falso)

                    else if (!packet.OneShot)
                    {
                        if (packet.Attempts < 3)
                        {

                            // riprovare dopo 1 secondo
                            packet.SendAfter = server.Now + 1.0f;
                            sendQueue.Enqueue(packet);
                        }
                    }
                }
                else
                {

                    //troppo tempo prima/ troppo veloce, riaccoda il pacchetto
                    sendQueue.Enqueue(packet);
                }
            }


            // controlla gli accessi nella tabella
            List<uint> deadPackets = new List<uint>();
            foreach (uint id in ackTab.Keys)
            {
                Packet packet = ackTab[id];
                if (packet.IsExpired(Server.Now))
                {
                    if (packet.Attempts < 3)
                    {
                        sendQueue.Enqueue(packet);
                    }
                    else
                    {
                        deadPackets.Add(id);
                    }
                }
            }

            foreach (uint id in deadPackets)
            {
                ackTab.Remove(id);
            }
        }

        

        public void Enqueue(Packet packet)
        {
            sendQueue.Enqueue(packet);
        }

        public override string ToString()
        {
            return endPoint.ToString();
        }
    }
}