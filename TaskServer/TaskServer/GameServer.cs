using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;


    //AGGIUNTE:
    //1 malus, gestione via avatar/server
    //2 WIP GameClient Logout
    //3 WIP GameObj removal


namespace TaskServer
{
    //Start
    public class GameServer
    {
        private delegate void GameCommand(byte[] data, EndPoint sender);

        private Dictionary<byte, GameCommand> commandsTab;

        private Dictionary<EndPoint, GameClient> clientsTab;
        private Dictionary<uint, GameObject> gameObjectsTab;

        public void Join(byte[] data, EndPoint sender)
        {

            // controlliamo se il client ha già joinato
            if (clientsTab.ContainsKey(sender))
            {
                GameClient badClient = clientsTab[sender];
                badClient.IncreaseMalus();
                return;
            }

            GameClient newClient = new GameClient(sender, this);
            clientsTab[sender] = newClient;
            
            Avatar avatar = Spawn<Avatar>(); //Avatar avatar = new Avatar(newClient.Server);
            avatar.SetOwner(newClient);
            Packet welcome = new Packet(1, avatar.ObjectType, avatar.Id, avatar.X, avatar.Y, avatar.Z);
            welcome.NeedAck = true;
            newClient.Enqueue(welcome);


            // spawano tutti gli oggetti che sono nel server nel nuovo client
            foreach (GameObject gameObject in gameObjectsTab.Values)
            {
                // ignora me stesso
                if (gameObject == avatar)
                    continue;
                Packet spawn = new Packet(2, gameObject.ObjectType, gameObject.Id, gameObject.X, gameObject.Y, gameObject.Z);//1 - 5*4
                spawn.NeedAck = true;
                newClient.Enqueue(spawn);
            }


            // informa gli altri clienti 
            //del nuovo
            Packet newClientSpawned = new Packet(2, avatar.ObjectType, avatar.Id, avatar.X, avatar.Y, avatar.Z);
            newClientSpawned.NeedAck = true;
            SendToAllClientsExceptOne(newClientSpawned, newClient);

            Console.WriteLine("client {0} joined with avatar {1}", newClient, avatar.Id);
        }

        public void Ack(byte[] data, EndPoint sender)
        {
            if (!clientsTab.ContainsKey(sender))
            {
                return;
            }

            GameClient client = clientsTab[sender];
            uint packetId = BitConverter.ToUInt32(data, 1);
            client.Ack(packetId);
        }

        public void Update(byte[] data, EndPoint sender)
        {
            if (!clientsTab.ContainsKey(sender))
            {
                return;
            }
            GameClient client = clientsTab[sender];
            uint netId = BitConverter.ToUInt32(data, 1);
            if (gameObjectsTab.ContainsKey(netId))
            {
                GameObject gameObject = gameObjectsTab[netId];
                if (gameObject.IsOwnedBy(client))
                {
                    float x = BitConverter.ToSingle(data, 5);
                    float y = BitConverter.ToSingle(data, 9);
                    float z = BitConverter.ToSingle(data, 13);
                    gameObject.SetPosition(x, y, z);
                }

                //aggiornamento di malus
                //comportamento errato
                else
                {
                    client.IncreaseMalus();
                }
            }
        }


        public void Exit(byte[] data, EndPoint sender)
        {
            // conttrollo se il client ha gia' joinato
            if (clientsTab.ContainsKey(sender))
            {
                GameClient badClient = clientsTab[sender];
                badClient.IncreaseMalus();
                return;
            }

            GameClient logOutClient = clientsTab[sender];

            // rimuovo tutti gli item del client
            foreach (GameObject gameObject in gameObjectsTab.Values)
            {
                if (gameObject.Owner == logOutClient)
                {
                    gameObjectsTab.Remove(gameObject.Id);

                    //se è il comando 4 distrugge  l'oggetto 
                    Packet removeItem = new Packet(4, gameObject);
                    removeItem.NeedAck = true;
                    SendToAllClients(removeItem);
                }
            }

            clientsTab.Remove(sender);
            //se è il comando 5 fa il logout al client
            Packet removeClient = new Packet(5, logOutClient);
            removeClient.NeedAck = true;
            SendToAllClients(removeClient);

            
        }


        private IMonotonicClock serverClock;
        public float Now { get { return serverClock.GetNow(); } }
        private float currentNow;

        private IGameTransport transport;

        public uint NumClients { get { return (uint)clientsTab.Count; } }
        public uint NumGameObjs { get { return (uint)gameObjectsTab.Count; } }

        public GameServer(IGameTransport transport, IMonotonicClock clock)
        {
            clientsTab = new Dictionary<EndPoint, GameClient>();
            gameObjectsTab = new Dictionary<uint, GameObject>();
            commandsTab = new Dictionary<byte, GameCommand>();
            commandsTab[0] = Join;
            commandsTab[3] = Update;

            commandsTab[7] = Exit;

            commandsTab[255] = Ack;

            serverClock = clock;
            this.transport = transport;
        }

        public void SingleStep()
        {
            currentNow = serverClock.GetNow();
            EndPoint sender = transport.CreateEndPoint();
            byte[] data = transport.Recv(256, ref sender);
            if (data != null)
            {
                byte gameCommand = data[0];
                if (commandsTab.ContainsKey(gameCommand))
                {
                    commandsTab[gameCommand](data, sender);
                }
            }

            foreach (GameClient client in clientsTab.Values)
            {
                client.Process();
            }

            foreach (GameObject gameObject in gameObjectsTab.Values)
            {
                gameObject.Tick();
            }

            if (checkMalusTimer <= Now)
            {
                CheckMalus();
            }
        }

        public void Start()
        {
            Console.WriteLine("server started");
            while (true)
            {
                SingleStep();
            }
        }

        private float checkMalusTimer = 0.0f;
        private float timeToWaitBeforeCheckClientsMalus = 25.0f;
        public void CheckMalus()
        {
            if (checkMalusTimer <= Now)
            {
                foreach (GameClient client in clientsTab.Values)
                {
                    uint serverMalus = client.Malus;
                    if (serverMalus > 0)
                    {
                        if (serverMalus >= 7)
                        {

                            // rimuove client con  valore di malus troppo elevato
                        }
                        else if (client.CanReduceClientMalus)
                            client.ReduceMalus();
                    }
                    else
                        continue;
                }
            }
            checkMalusTimer = Now + timeToWaitBeforeCheckClientsMalus;
        }

        

        public bool Send(Packet packet, EndPoint endPoint)
        {
            return transport.Send(packet.GetData(), endPoint);
        }
        public void SendToAllClients(Packet packet)
        {
            foreach (GameClient client in clientsTab.Values)
            {
                client.Enqueue(packet);
            }
        }
        public void SendToAllClientsExceptOne(Packet packet, GameClient except)
        {
            foreach (GameClient client in clientsTab.Values)
            {
                if (client != except)
                    client.Enqueue(packet);
            }
        }

        public void RegisterGameObject(GameObject gameObject)
        {
            if (gameObjectsTab.ContainsKey(gameObject.Id))
                throw new Exception("GameObject already registered");
            gameObjectsTab[gameObject.Id] = gameObject;

        }

        public T Spawn<T>() where T : GameObject
        {
            object[] ctorParams = { this };

            T newGameObject = Activator.CreateInstance(typeof(T), ctorParams) as T;
            RegisterGameObject(newGameObject);
            return newGameObject;
        }

        public GameObject GetGameObj(uint objId)
        {
            if (gameObjectsTab.ContainsKey(objId))
                return gameObjectsTab[objId];
            else
                return null;
        }
    }
}

