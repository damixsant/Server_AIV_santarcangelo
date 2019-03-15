using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TaskServer.Test
{
    public class TestGameServer
    {
        public TestGameServer()
        {
        }

        private FakeTransport transport;
        private FakeClock clock;
        private GameServer server;

        // Viene chiamato prima di ogni test
        [SetUp]
        public void SetUp()
        {
            transport = new FakeTransport();
            clock = new FakeClock();
            server = new GameServer(transport, clock);
        }


        // Test - se un nuovo server
        // ha il serverClock a zero
        [Test]
        public void TestGameServerGreenLigthZero()
        {
            Assert.That(server.Now, Is.EqualTo(0));
        }
        [Test]
        public void TestGameServerRedLigthZero()
        {
            Assert.That(server.Now, Is.Not.EqualTo(1));
        }


        // Test - se un nuovo server non ha nessun GameClient collegato
        [Test]
        public void TestGreenLigthClientOnStart()
        {
            Assert.That(server.NumClients, Is.EqualTo(0));
        }
        [Test]
        public void TestRedLigthClientOnStart()
        {
            Assert.That(server.NumClients, Is.Not.EqualTo(1));
        }


        // Verifica se un nuovo server non ha gameobject all'interno
        [Test]
        public void TestGreenLigthObjOnStart()
        {
            Assert.That(server.NumClients, Is.EqualTo(0));
        }
        [Test]
        public void TestRedLigthObjOnStart()
        {
            Assert.That(server.NumClients, Is.Not.EqualTo(1));
        }


        // Test - numero di join del client
        [Test]
        public void TestGreenLigthJoinNumOfClients()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            Assert.That(server.NumClients, Is.EqualTo(1));
        }
        [Test]
        public void TestRedLigthJoinNumOfClients()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            Assert.That(server.NumClients, Is.Not.EqualTo(0));
        }


        // Test - numero di join dei gameobj
        [Test]
        public void TestGreenLigthJoinNumOfGameObj()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            Assert.That(server.NumGameObjs, Is.EqualTo(1));
        }
        [Test]
        public void TestRedLigthJoinNumOfGameObj()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            Assert.That(server.NumGameObjs, Is.Not.EqualTo(0));
        }

        //Test - Welcome
        [Test]
        public void TestWelcomeAfterJoinGreenLigth()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            FakeData welcome = transport.ClientDequeue();
            Assert.That(welcome.data[0], Is.EqualTo(1));
        }
        [Test]
        public void TestWelcomeAfterJoinRedLigth()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            FakeData welcome = transport.ClientDequeue();
            Assert.That(welcome.data[0], Is.Not.EqualTo(0));
        }

        //Test - creazione Avatar dopo il join
        [Test]
        public void TestSpawnAvatarAfterJoinGreenLigth()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientDequeue();
            Assert.That(() => transport.ClientDequeue(), Throws.InstanceOf<FakeQueueEmpty>());
        }


        // Test - join diversi dallo stesso indirizzo
        [Test]
        public void TestJoinSameAddressMultipleClientGreenLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "tester", 1);
            server.SingleStep();
            Assert.That(server.NumClients, Is.EqualTo(2));
        }
        [Test]
        public void TestJoinSameAddressMultipleClientRedLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "tester", 1);
            server.SingleStep();
            Assert.That(server.NumClients, Is.Not.EqualTo(1));
        }

        //Test - join diversi dalla stessa porta
        [Test]
        public void TestJoinSamePortMultipleClientGreenLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "foobar", 0);
            server.SingleStep();
            Assert.That(server.NumClients, Is.EqualTo(2));
        }
        [Test]
        public void TestJoinSamePortMultipleClientRedLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "foobar", 0);
            server.SingleStep();
            Assert.That(server.NumClients, Is.Not.EqualTo(1));
        }

        //Test - join diversi dalla stessa porta e indirizzi
        [Test]
        public void TestJoinSameAddressSamePortMultipleClientGreenLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            Assert.That(server.NumClients, Is.EqualTo(1));
        }
        [Test]
        public void TestJoinSameAddressSamePortMultipleClientRedLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            Assert.That(server.NumClients, Is.Not.EqualTo(0));
        }

        //Test - join diversi (generici)
        [Test]
        public void TestJoinTwoClientsWelcomeGreenLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();

            Assert.That(transport.ClientQueueCount, Is.EqualTo(5));

            Assert.That(transport.ClientDequeue().endPoint.Addr, Is.EqualTo("tester"));
            Assert.That(transport.ClientDequeue().endPoint.Addr, Is.EqualTo("tester"));
            Assert.That(transport.ClientDequeue().endPoint.Addr, Is.EqualTo("tester"));
            Assert.That(transport.ClientDequeue().endPoint.Addr, Is.EqualTo("foobar"));
            Assert.That(transport.ClientDequeue().endPoint.Addr, Is.EqualTo("foobar"));

        }
        [Test]
        public void TestJoinTwoClientsWelcomeRedLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();

            Assert.That(transport.ClientQueueCount, Is.EqualTo(5));

            Assert.That(transport.ClientDequeue().endPoint.Addr, Is.Not.EqualTo("foobar"));
            Assert.That(transport.ClientDequeue().endPoint.Addr, Is.Not.EqualTo("foobar"));
            Assert.That(transport.ClientDequeue().endPoint.Addr, Is.Not.EqualTo("foobar"));
            Assert.That(transport.ClientDequeue().endPoint.Addr, Is.Not.EqualTo("tester"));
            Assert.That(transport.ClientDequeue().endPoint.Addr, Is.Not.EqualTo("tester"));
        }

        //[Test]
        //public void Ack()
        //{
        //    FakeEndPoint fakeEndPoint = new FakeEndPoint("teste", 0);
        //    GameClient client = new GameClient(server, fakeEndPoint);

        //    Packet packet = new Packet(1,1,1,0,0,0);
        //    packet.NeedAck = true;

        //    client.Enqueue(packet);
        //    client.Process();



        //}



        //Test - malus
        [Test]
        public void TestMalusUpdateGreenLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);
            // riavvia GameObj da avatarId e controlla la sua posizione precedente
            Avatar testerObj = (Avatar)server.GetGameObject(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            // controlla GameClient malus alla creazione
            uint testerMalus = testerObj.Malus;
            Assert.That(testerMalus, Is.EqualTo(0));
            // controlla GameClient malus alla creazione
            uint malusValue = 3;
            testerObj.IncreaseMalus(malusValue);
            uint newTesterMalus = testerObj.Malus;
            Assert.That(newTesterMalus, Is.EqualTo(testerMalus + malusValue));
        }
        [Test]
        public void TestMalusUpdateRedLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);

            // riavvia GameObj da avatarId e controlla la sua posizione precedente
            Avatar testerObj = (Avatar)server.GetGameObject(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            // controlla GameClient malus alla creazione
            uint testerMalus = testerObj.Malus;
            Assert.That(testerMalus, Is.EqualTo(0));
            // controlla GameClient malus alla creazione
            uint malusValue = 3;
            testerObj.IncreaseMalus(malusValue);
            uint newTesterMalus = testerObj.Malus;
            Assert.That(newTesterMalus, Is.Not.EqualTo(testerMalus));
        }


        // Test - aumento del malus dopo un cattivo comportamento
        [Test]
        public void TestEvilMalusUpdateGreenLight()
        {
            // TODO ottiene l'id dai pacchetti di benvenuto
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);
            // riavvia GameObj da avatrId e memorizza la sua posizione
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();
            // riavvia GameObj da avatarId e controlla la sua posizione precedente
            Avatar testerObj = (Avatar)server.GetGameObject(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            // controlla GameClient malus alla creazione
            uint testerMalus = testerObj.Malus;
            Assert.That(testerMalus, Is.EqualTo(0));
            //prende la posizione della parentela (posizione di base)
            float startX = testerObj.X;
            float startY = testerObj.Y;
            float startZ = testerObj.Z;
            float offsetX = 10.0f;
            float offsetY = 20.0f;
            float offsetZ = 30.0f;
            Packet movePacket = new Packet(3, avatarId, offsetX, offsetY, offsetZ);
            transport.ClientEnqueue(movePacket, "foobar", 1);
            server.SingleStep();
            // prende una nuova posizione dopo il movimento
            float newX = testerObj.X;
            float newY = testerObj.Y;
            float newZ = testerObj.Z;
            // controlla la posizione precedente e dopo il movimento; 
            // il valore dovrebbe essere lo stesso perché il movimento non dovrebbe accadere
            Assert.That(startX, Is.EqualTo(newX));
            Assert.That(startY, Is.EqualTo(newY));
            Assert.That(startZ, Is.EqualTo(newZ));
            // controlla il malus GameClient dopo un cattivo comportamento
            uint newTesterMalus = testerObj.Malus;
            Assert.That(newTesterMalus, Is.EqualTo(testerMalus + 1));
        }
        [Test]
        public void TestEvilMalusUpdateRedLight()
        {
            // TODO ottiene l'id dai pacchetti di benvenuto
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);
            // riavvia GameObj da avatrId e memorizza la sua posizione
            transport.ClientEnqueue(packet, "foobar", 1);
            server.SingleStep();
            // riavvia GameObj da avatarId e controlla la sua posizione precedente
            Avatar testerObj = (Avatar)server.GetGameObject(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            // controlla GameClient malus alla creazione
            uint testerMalus = testerObj.Malus;
            Assert.That(testerMalus, Is.EqualTo(0));
            //prende la posizione della parentela (posizione di base)
            float startX = testerObj.X;
            float startY = testerObj.Y;
            float startZ = testerObj.Z;
            float offsetX = 10.0f;
            float offsetY = 20.0f;
            float offsetZ = 30.0f;
            Packet movePacket = new Packet(3, avatarId, offsetX, offsetY, offsetZ);
            transport.ClientEnqueue(movePacket, "foobar", 1);
            server.SingleStep();
            // prende una nuova posizione dopo il movimento
            float newX = testerObj.X;
            float newY = testerObj.Y;
            float newZ = testerObj.Z;
            // controlla la posizione precedente e dopo il movimento; 
            // il valore dovrebbe essere lo stesso perché il movimento non dovrebbe accadere
            Assert.That(newX, Is.Not.EqualTo(startX + offsetX));
            Assert.That(newY, Is.Not.EqualTo(startY + offsetY));
            Assert.That(newZ, Is.Not.EqualTo(startZ + offsetZ));
            // controlla il malus GameClient dopo un cattivo comportamento
            uint newTesterMalus = testerObj.Malus;
            Assert.That(newTesterMalus, Is.Not.EqualTo(testerMalus));
        }

        //Test - riduzione del malus
        [Test]
        public void TestMalusReductionUpdateGreenLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);


            // riavvia GameObj da avatarId e controlla la sua posizione precedente
            Avatar testerObj = (Avatar)server.GetGameObject(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));

            // controlla GameClient malus alla creazione
            uint testerMalus = testerObj.Malus;
            Assert.That(testerMalus, Is.EqualTo(0));
            // controlla GameClient malus alla creazione
            uint malusValue = 3;
            testerObj.IncreaseMalus(malusValue);
            uint testerMalusAfterBadBehaviour = testerObj.Malus;
            Assert.That(testerMalusAfterBadBehaviour, Is.EqualTo(testerMalus + malusValue));
            clock.IncreaseTimeStamp(400f);
            server.CheckMalus();
            uint newMalus = testerObj.Malus;
            Assert.That(newMalus, Is.EqualTo(testerMalusAfterBadBehaviour - 1));
            clock.IncreaseTimeStamp(30f);
            Assert.That(testerObj.Malus, Is.EqualTo(newMalus));
        }
        [Test]
        public void TestMalusReductionUpdateRedLight()
        {
            Packet packet = new Packet(0);
            transport.ClientEnqueue(packet, "tester", 0);
            server.SingleStep();
            uint avatarId = BitConverter.ToUInt32(transport.ClientDequeue().data, 5);
            // riavvia GameObj da avatarId e controlla la sua posizione precedente
            Avatar testerObj = (Avatar)server.GetGameObject(avatarId);
            Assert.That(testerObj, Is.Not.EqualTo(null));
            // controlla GameClient malus alla creazione
            uint testerMalus = testerObj.Malus;
            Assert.That(testerMalus, Is.EqualTo(0));
            // controlla GameClient malus alla creazione
            uint malusValue = 3;
            testerObj.IncreaseMalus(malusValue);
            uint testerMalusAfterBadBehaviour = testerObj.Malus;
            Assert.That(testerMalusAfterBadBehaviour, Is.EqualTo(testerMalus + malusValue));
            clock.IncreaseTimeStamp(400f);
            server.CheckMalus();
            uint newMalus = testerObj.Malus;
            Assert.That(newMalus, Is.Not.EqualTo(testerMalusAfterBadBehaviour));
            clock.IncreaseTimeStamp(30f);
            Assert.That(testerObj.Malus, Is.EqualTo(newMalus));
        }



        }
}


