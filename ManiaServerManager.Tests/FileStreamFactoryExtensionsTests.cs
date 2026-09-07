using System.IO.Abstractions;
using System.Text;

namespace ManiaServerManager.Tests;

public sealed class FileStreamFactoryExtensionsTests
{
    [Fact]
    public async Task NewWriteAsync_CreatesAndOverwritesFile()
    {
        using var directory = new TempDirectory();
        var path = Path.Combine(directory.Path, "file.txt");
        await File.WriteAllTextAsync(path, "old content");
        var fileSystem = new FileSystem();

        await using (var stream = fileSystem.FileStream.NewWriteAsync(path))
        {
            await stream.WriteAsync(Encoding.UTF8.GetBytes("new"));
        }

        Assert.Equal("new", await File.ReadAllTextAsync(path));
    }

    [Fact]
    public async Task NewReadAsync_OpensExistingFileForAsynchronousReading()
    {
        using var directory = new TempDirectory();
        var path = Path.Combine(directory.Path, "file.txt");
        await File.WriteAllTextAsync(path, "content");
        var fileSystem = new FileSystem();

        await using var stream = fileSystem.FileStream.NewReadAsync(path);
        using var reader = new StreamReader(stream);

        Assert.True(stream.CanRead);
        Assert.Equal("content", await reader.ReadToEndAsync());
    }

    [Fact]
    public void NewReadAsync_MissingFileThrows()
    {
        using var directory = new TempDirectory();
        var fileSystem = new FileSystem();

        Assert.Throws<FileNotFoundException>(() => fileSystem.FileStream.NewReadAsync(Path.Combine(directory.Path, "missing.txt")));
    }
}
