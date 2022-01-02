using System;

namespace NVorbis.Contracts.Ogg
{
    interface IPacketReader
    {
		Sandbox.Memory<byte> GetPacketData(int pagePacketIndex);

        void InvalidatePacketCache(IPacket packet);
    }
}
