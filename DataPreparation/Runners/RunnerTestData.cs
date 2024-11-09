using DataPreparation.Models;

namespace DataPreparation.Runners;

public static class RunnerTestData
{
    public static void Up(List<PreparedData> testData)
    {
        foreach (var data in testData)
        {
            data.RunUp().Wait();
        }
    }

    public static void Down(List<PreparedData> testData)
    {
        foreach (var data in testData)
        {
            data.RunDown().Wait();
        }
    }
}