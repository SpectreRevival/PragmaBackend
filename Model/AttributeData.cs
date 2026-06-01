namespace Model;

public interface IAttributeData<ModelType, PacketType>
{
    /** 
     * Syncs the data to the database given a key
     */
    public void SyncToDatabase(string key);

    public abstract static ModelType GetFromDatabase(string key);

    public PacketType ToPacketType();
}