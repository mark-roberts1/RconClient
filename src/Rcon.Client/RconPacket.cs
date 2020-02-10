using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rcon.Client
{
    /// <summary>
    /// Designed per the spec from https://developer.valvesoftware.com/wiki/Source_RCON_Protocol
    /// </summary>
    public struct RconPacket
    {
        public RconPacket(int commandId, int packetType, string command)
        {
            if (packetType != CommandTypes.SERVERDATA_AUTH
                && packetType != CommandTypes.SERVERDATA_AUTHRESPONSE
                && packetType != CommandTypes.SERVERDATA_EXECCOMMAND
                && packetType != CommandTypes.SERVERDATA_RESPONSE_VALUE)
            {
                throw new ArgumentException("packetType was not recognized.");
            }

            command = command ?? string.Empty;

            // From the spec:
            // Since the only one of these values that can change in length is the body, 
            // an easy way to calculate the size of a packet is to find the byte-length 
            // of the packet body, then add 10 to it.
            Size = 10 + Encoding.UTF8.GetByteCount(command);
            CommandId = commandId;
            PacketType = packetType;
            Body = command;
        }

        public int Size { get; }
        public int CommandId { get; }
        public int PacketType { get; }
        public string Body { get; }

        public bool IsResponseTerminator
        {
            get
            {
                return Body == "Unknown request 0";
            }
        }

        public byte[] GetBytes()
        {
            // byte size will be Size + 4, since Size does not include itself.
            var bytes = new byte[Size + 4];
            var sizeBytes = BitConverter.GetBytes(Size);
            var commandIdBytes = BitConverter.GetBytes(CommandId);
            var packetTypeBytes = BitConverter.GetBytes(PacketType);
            var bodyBytes = Encoding.UTF8.GetBytes(Body);

            // Size
            bytes[0] = sizeBytes[0];
            bytes[1] = sizeBytes[1];
            bytes[2] = sizeBytes[2];
            bytes[3] = sizeBytes[3];

            // Id
            bytes[4] = commandIdBytes[0];
            bytes[5] = commandIdBytes[1];
            bytes[6] = commandIdBytes[2];
            bytes[7] = commandIdBytes[3];

            // Type
            bytes[8] = packetTypeBytes[0];
            bytes[9] = packetTypeBytes[1];
            bytes[10] = packetTypeBytes[2];
            bytes[11] = packetTypeBytes[3];

            int j = 0;

            for (int i = 0; i < bodyBytes.Length; i++)
            {
                j = i + 12;

                bytes[j] = bodyBytes[i];
            }

            bytes[j + 1] = 0x0;
            bytes[j + 2] = 0x0;

            return bytes;
        }

        public static RconPacket CommandTerminator(int commandId)
        {
            return new RconPacket(commandId, CommandTypes.SERVERDATA_RESPONSE_VALUE, string.Empty);
        }

        public static RconPacket From(int commandId, IRconCommand rconCommand)
        {
            rconCommand.ThrowIfNull();

            return new RconPacket(commandId, rconCommand.CommandType, rconCommand.Text);
        }

        public static RconPacket From(BinaryReader reader)
        {
            try
            {
                var len = reader.ReadInt32();
                var commandId = reader.ReadInt32();
                var type = reader.ReadInt32();
                var data = len > 10 ? reader.ReadBytes(len - 10) : new byte[] { };
                var pad = reader.ReadBytes(2);

                return new RconPacket(commandId, type, Encoding.UTF8.GetString(data));
            }
            catch (Exception ex)
            {
                throw new PacketMalformedException(ex);
            }
        }
    }
}
