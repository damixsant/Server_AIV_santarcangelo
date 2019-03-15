﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskServer
{
    public abstract class GameObject
    {
        private float x;
        public float X { get { return x; } }
        private float y;
        public float Y { get { return y; } }
        private float z;
        public float Z { get { return z; } }


        // Oggetti che prendono il server dal loro padre/propetario
        private GameClient owner;
        public GameClient Owner { get { return owner; } }
        private GameServer server;
        public GameServer Server { get { return server; } }

        public bool IsOwnedBy(GameClient client)
        {
            return owner == client;
        }

        public void SetOwner(GameClient client)
        {
            owner = client;
        }

        private uint internalObjectType;
        public uint ObjectType
        {
            get
            {
                return internalObjectType;

            }
        }

        private static uint gameObjectCounter;
        private uint internalId;
        public uint Id { get { return internalId; } }

        public virtual void Tick() { }


        // modifichiamo l'instanziatore per avere un GameServer gia' dall'inizio
        public GameObject(uint objectType, GameServer server)
        {
            internalObjectType = objectType;
            internalId = ++gameObjectCounter;
            this.server = server;
            Console.WriteLine("spawned GameObj {0} of type {1}", Id, ObjectType);
        }

        public GameObject(uint objectType, GameClient client)
        {
            internalObjectType = objectType;
            internalId = ++gameObjectCounter;
            SetOwner(client);
            server = owner.Server;
            Console.WriteLine("spawned GameObj {0} of type {1}", Id, ObjectType);
        }

        public void SetPosition(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        

        
    }
}