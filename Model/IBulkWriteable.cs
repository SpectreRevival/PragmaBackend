using Npgsql;

namespace Model;

public interface IBulkWriteable
{
    public static abstract NpgsqlBinaryImporter CreateBulkWriter();
    Task WriteToBulkWriter(NpgsqlBinaryImporter importer);
}