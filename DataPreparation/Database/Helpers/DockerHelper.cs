using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DataPreparation.Database.Helpers;

/// <summary>
/// Helper class for managing Docker PostgreSQL database operations such as backup and restore.
/// Executes Docker CLI commands to interact with a running PostgreSQL container.
/// </summary>
public class DockerHelper
{
    private readonly string _containerID;
    private readonly string _databaseName;
    private readonly string _userName;
    private readonly string _password;
    private readonly string _baseBackupFilePath = "/tmp/DataPreparationBackup_{0}.dump";
    private readonly string _backupFilePath;
    private readonly ILogger<DockerHelper> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockerHelper"/> class.
    /// This constructor is kept for backwards compatibility.
    /// </summary>
    /// <param name="containerId">The Docker container ID</param>
    /// <param name="databaseName">The name of the PostgreSQL database</param>
    /// <param name="userName">The database user name</param>
    /// <param name="password">The database password</param>
    public DockerHelper(string containerId, string databaseName, string userName, string password)
        : this(containerId, databaseName, userName, password, Microsoft.Extensions.Logging.Abstractions.NullLogger<DockerHelper>.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockerHelper"/> class.
    /// </summary>
    /// <param name="containerId">The Docker container ID</param>
    /// <param name="databaseName">The name of the PostgreSQL database</param>
    /// <param name="userName">The database user name</param>
    /// <param name="password">The database password</param>
    /// <param name="logger">Logger instance for structured logging</param>
    public DockerHelper(string containerId, string databaseName, string userName, string password, ILogger<DockerHelper> logger)
    {
        _containerID = containerId;
        _databaseName = databaseName;
        _userName = userName;
        _password = password;
        _logger = logger;
        _backupFilePath = string.Format(_baseBackupFilePath, _databaseName);
    }

    /// <summary>
    /// Creates a backup of the database inside the Docker container.
    /// </summary>
    /// <returns>True if the backup was successful; otherwise, false.</returns>
    public bool BackupDatabaseInDocker()
    {
        _logger.LogInformation("Database backup started for {DatabaseName}", _databaseName);
        var args = $"exec {_containerID} pg_dump -U {_userName} -F c -b -v -f {_backupFilePath} {_databaseName}";
        var ret = ExecuteCommand("docker", args, out _, out _);
        _logger.LogInformation("Database backup completed for {DatabaseName}", _databaseName);
        return ret;
    }

    /// <summary>
    /// Restores the database from a backup inside the Docker container.
    /// </summary>
    /// <returns>True if the restore was successful; otherwise, false.</returns>
    public bool RestoreDatabaseInDocker()
    {
        _logger.LogInformation("Database restore started for {DatabaseName}", _databaseName);
        var args = $"exec  {_containerID} pg_restore -U {_userName} -d {_databaseName} -v {_backupFilePath}";
        var ret = ExecuteCommand("docker", args, out _, out string errorOut);
        _logger.LogInformation("Database restore completed for {DatabaseName}", _databaseName);
        if (!ret && errorOut.Contains("pg_restore: warning: errors ignored on restore:"))
        {
            return true;
        }
        return ret;
    }

    /// <summary>
    /// Checks if the Docker container exists and is running.
    /// </summary>
    /// <returns>True if the container is running; otherwise, false.</returns>
    public bool CheckIfDockerExists()
    {
        string args = $"ps -a --filter \"id={_containerID}\"   --filter \"status=running\"  --format \"{{{{.ID}}}}\"";

        ExecuteCommand("docker", args, out string output, out _);

        return !string.IsNullOrWhiteSpace(output);
    }

    /// <summary>
    /// Checks if a backup file exists inside the Docker container.
    /// </summary>
    /// <returns>True if the backup file exists; otherwise, false.</returns>
    public bool CheckIfBackupExists()
    {
       string args = $"exec {_containerID} ls {_backupFilePath}";
       return ExecuteCommand("docker", args, out _, out _);
    }

    private bool ExecuteCommand(string command, string args, out string output, out string error)
    {
        output = string.Empty;
        error = string.Empty;

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
            _logger.LogTrace("Docker output: {Output}", e.Data);
            outputBuilder.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            _logger.LogWarning("Docker error: {Error}", e.Data);
            errorBuilder.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        var ret = process.WaitForExit(15000);
        if (ret)
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
