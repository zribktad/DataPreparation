namespace DataPreparation.Data;

public interface  IBeforePreparationTask 
{
    Task UpData();
    Task DownData();
}