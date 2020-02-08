using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using System.IO;

namespace Rcon.Client.UnitTests
{
    [TestFixture]
    public class RconPacketTests
    {
        readonly Mock<IRconCommand> _mockCommand = new Mock<IRconCommand>();
        Stream stream;

        [SetUp]
        public void Setup()
        {
            stream = new MemoryStream();
        }

        [TearDown]
        public void TearDown()
        {
            stream.Dispose();
        }

        [Test]
        public void TestThatCommandTerminatorFactoryCreatesEmptyPacket()
        {
            // Arrange
            int commandId = GetRandomInt();

            // Act
            var terminator = RconPacket.CommandTerminator(commandId);

            // Assert
            Assert.That(terminator.Body == string.Empty);
            Assert.That(terminator.Size == 10);
        }

        [TestCase(0, "do something")]
        [TestCase(2, "do something else")]
        [TestCase(3, "do something else else")]
        public void TestThatRconPacketIsConstructedProperlyWithFactoryMethod(int commandType, string command)
        {
            // Arrange
            int commandId = GetRandomInt();

            _mockCommand.Setup(s => s.CommandType).Returns(commandType);
            _mockCommand.Setup(s => s.Text).Returns(command);

            // Act
            var packet = RconPacket.From(commandId, _mockCommand.Object);

            // Assert
            Assert.That(packet.Size == 10 + Encoding.UTF8.GetByteCount(command));
            Assert.That(packet.CommandId == commandId);
            Assert.That(packet.PacketType == commandType);
            Assert.That(packet.Body == command);
        }

        [TestCase(0, "do something")]
        [TestCase(2, "do something else")]
        [TestCase(3, "do something else else")]
        public void TestThatFactoryMethodAndGetBytesFunctionCorrectly(int commandType, string command)
        {
            // Arrange
            int commandId = GetRandomInt();

            _mockCommand.Setup(s => s.CommandType).Returns(commandType);
            _mockCommand.Setup(s => s.Text).Returns(command);

            // Act
            var packet1 = RconPacket.From(commandId, _mockCommand.Object);

            using var writer = new BinaryWriter(stream);
            
            writer.Write(packet1.GetBytes());
            writer.Flush();

            stream.Seek(0, SeekOrigin.Begin);
            
            using var reader = new BinaryReader(stream);

            var packet2 = RconPacket.From(reader);

            // Assert
            Assert.That(packet1.Size == packet2.Size
                && packet1.CommandId == packet2.CommandId
                && packet1.PacketType == packet2.PacketType
                && packet1.Body == packet2.Body);
        }

        int GetRandomInt() => new Random().Next();
    }
}
