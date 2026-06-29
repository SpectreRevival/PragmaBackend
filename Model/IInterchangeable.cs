namespace Model;

public interface IInterchangeable<ModelClass, PacketClass>
{
    static abstract ModelClass FromPacket(PacketClass inst);
    abstract PacketClass ToPacket();
}

public interface IInterchangeableKeyed<ModelClass, PacketClass, KeyType> : IKeyed<KeyType>
{
    static abstract ModelClass FromPacket(PacketClass inst, KeyType key);
    abstract PacketClass ToPacket();
}