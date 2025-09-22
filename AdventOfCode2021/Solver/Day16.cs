using System.Numerics;

namespace AdventOfCode2021.Solver;

internal partial class Day16 : BaseSolver
{
    public override string PuzzleTitle { get; } = "Packet Decoder";

    private sealed class Packet
    {
        public int Version { get; set; } = -1;
        public int TypeId { get; set; } = -1;
        public long Literal { get; set; } = 0;
        public List<Packet> Content { get; set; } = [];

        public long Value => TypeId switch
        {
            0 => Content.Sum(x => x.Value),                        // Sum
            1 => Content.Aggregate(1L, (acc, x) => acc * x.Value), // Product
            2 => Content.Min(x => x.Value),                        // Minimum
            3 => Content.Max(x => x.Value),                        // Maximum
            4 => Literal,                                          // Literal value
            5 => Content[0].Value > Content[1].Value ? 1 : 0,      // Greater than
            6 => Content[0].Value < Content[1].Value ? 1 : 0,      // Less than
            7 => Content[0].Value == Content[1].Value ? 1 : 0,     // Equal to
            _ => throw new InvalidOperationException($"Unknown TypeId {TypeId}"),
        };

        public long VersionSum => Version + Content.Sum(x => x.VersionSum);
    }

    private Packet _packet = new();

    public override string GetSolution1(bool isChallenge)
    {
        ExtractData();
        return _packet.VersionSum.ToString();
    }

    public override string GetSolution2(bool isChallenge)
    {
        ExtractData();
        return _packet.Value.ToString();
    }

    private void ExtractData()
    {
        // Convert string into LongInteger to have full data in one variable
        BigInteger _fullData = 0;
        int _nbrOfBits = 0;
        foreach (char currentCharacter in _puzzleInput[0])
        {
            int characterValue = char.IsDigit(currentCharacter)
                ? currentCharacter - '0'
                : 10 + char.ToUpper(currentCharacter) - 'A'; // 10 is added because of digits 0 to 9 who are before

            _fullData <<= 4;
            _fullData |= characterValue;
            _nbrOfBits += 4;
        }

        // Mirror bits to have the bit to analyze as LSB
        _fullData = MirrorBits(_fullData, _nbrOfBits);

        // Extract packets
        _packet = GetPacketsFromMirroredData(_fullData).packets[0];
    }

    private static (List<Packet> packets, int consumedBits) GetPacketsFromMirroredData(BigInteger fullData)
    {
        // Get packet version and typeId
        Packet newPacket = new()
        {
            Version = (int)MirrorBits(fullData & 0b111, 3),
            TypeId = (int)MirrorBits((fullData >> 3) & 0b111, 3),
        };
        fullData = fullData >> 6;
        int consumedBits = 6;

        // Literal type ?
        if (newPacket.TypeId == 4)
        {
            bool lastBlock;
            do
            {
                lastBlock = (fullData & 0b1) == 0;
                newPacket.Literal = (newPacket.Literal << 4) + (long)MirrorBits((fullData >> 1) & 0b1111, 4);
                fullData = fullData >> 5;
                consumedBits += 5;
            } while (!lastBlock);
            return ([newPacket], consumedBits);
        }

        // Operation type
        int lengthTypeId = (int)(fullData & 0b1);
        switch (lengthTypeId)
        {
            case 0:
                long subPacketLength = (long)MirrorBits((fullData >> 1) & 0b111111111111111, 15);
                fullData = fullData >> 16;
                consumedBits += 16;

                // Scan until packet length is reached
                while (subPacketLength > 0)
                {
                    (List<Packet> subPackets, int subPacketsBitSize) = GetPacketsFromMirroredData(fullData);
                    fullData = fullData >> subPacketsBitSize;
                    consumedBits += subPacketsBitSize;
                    subPacketLength -= subPacketsBitSize;
                    newPacket.Content.AddRange(subPackets);
                }
                break;

            case 1:
                int nbrOfPackets = (int)MirrorBits((fullData >> 1) & 0b11111111111, 11);
                fullData = fullData >> 12;
                consumedBits += 12;

                // Scan until number of packets is reached
                for (int packetNbr = 0; packetNbr < nbrOfPackets; packetNbr++)
                {
                    (List<Packet> subPackets, int subPacketsBitSize) = GetPacketsFromMirroredData(fullData);
                    fullData = fullData >> subPacketsBitSize;
                    consumedBits += subPacketsBitSize;
                    newPacket.Content.AddRange(subPackets);
                }
                break;
        }

        return ([newPacket], consumedBits);
    }

    private static BigInteger MirrorBits(BigInteger value, int bitCount)
    {
        BigInteger result = 0;
        for (int i = 0; i < bitCount; i++)
        {
            result = (result << 1) | (value & 1);
            value >>= 1;
        }
        return result;
    }
}