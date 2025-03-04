namespace DataPreparation.Testing;

public interface IDataPreparationSetUpConnections
{
    public static abstract IEnumerable<DataBaseConnection> SetUpConnections();
}