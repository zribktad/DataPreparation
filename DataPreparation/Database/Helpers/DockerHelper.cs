using System.Diagnostics;
using System.Text;

namespace DataPreparation.Database.Helpers;

public class DockerHelper
{
    private readonly string _containerID;
    private readonly string _databaseName;
    private readonly string _userName;
    private readonly string _password;
    private readonly string _baseBackupFilePath = "/tmp/DataPreparationBackup_{0}.dump";
    private readonly string _backupFilePath;

    public DockerHelper(string containerId, string databaseName, string userName, string password)
    {
        _containerID = containerId;
        _databaseName = databaseName;
        _userName = userName;
        _password = password;
        _backupFilePath = string.Format(_baseBackupFilePath, _databaseName);
    }

    public bool BackupDatabaseInDocker()
    {
        
        Console.WriteLine($"Database backup started for {_databaseName}");
        var args = $"exec {_containerID} pg_dump -U {_userName} -F c -b -v -f {_backupFilePath} {_databaseName}";
        var ret = ExecuteCommand("docker", args, out _, out _);
        Console.WriteLine($"Database backup completed for {_databaseName}");
        return ret;
        // // Copy the backup file from Docker to local
        // string copyCommand = $"docker cp {_containerID}:{_backupFilePath} {backupFilePath}";
        // ExecuteCommand(copyCommand);
    }

    public bool RestoreDatabaseInDocker()
    {
        // // Copy the backup file from local to Docker container
        // string copyCommand = $"docker cp {backupFilePath} {_containerID}:/tmp/backup.dump";
        // ExecuteCommand(copyCommand);
        Console.WriteLine($"Database restore started for {_databaseName}");
        var args = $"exec  {_containerID} pg_restore -U {_userName} -d {_databaseName} -v {_backupFilePath}";
        var ret = ExecuteCommand("docker",args, out _, out string errorOut);
        Console.WriteLine($"Database restore completed for {_databaseName}");
        if (!ret && errorOut.Contains("pg_restore: warning: errors ignored on restore:"))
        {
            return true;
        }
        return ret;
    }

    public bool CheckIfDockerExists()
    {
        string args = $"ps -a --filter \"id={_containerID}\"   --filter \"status=running\"  --format \"{{{{.ID}}}}\"";
         
        ExecuteCommand("docker",args, out string output, out _);
        
        if (string.IsNullOrEmpty(output))
        {
            return false;
        }
        return true;
    }
    
    public bool CheckIfBackupExists()
    {
       string args = $"exec {_containerID} ls {_backupFilePath}";
       return ExecuteCommand("docker",args, out _, out _);
    }

    private bool ExecuteCommand(string command,string args, out string output, out string error)
    {
        
        output = String.Empty;
        error = String.Empty;
        
        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = new Process();
        StringBuilder outputBuilder = new StringBuilder();
        StringBuilder errorBuilder = new StringBuilder();
        process.StartInfo = processStartInfo;
        process.OutputDataReceived += (sender, e) =>
        {
            Console.WriteLine(e.Data);
            outputBuilder.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            Console.WriteLine("Warning: " + e.Data);
            errorBuilder.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
       var ret = process.WaitForExit(15000);
       if(ret)
        {
            output = outputBuilder.ToString();
            error = errorBuilder.ToString();
            if (process.ExitCode == 0)
            {
                return true;
            }
        }
        
      
        return false;  
    }
}
