using System.Globalization;

namespace ManiaServerManager.Tests;

public sealed class BytesTests
{
    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(999, "999 B")]
    [InlineData(1_000, "1.00 KB")]
    [InlineData(999_999, "1000.00 KB")]
    [InlineData(1_000_000, "1.00 MB")]
    [InlineData(999_999_999, "1000.00 MB")]
    [InlineData(1_000_000_000, "1.00 GB")]
    public void Format_ReturnsExpectedUnit(long value, string expected)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Assert.Equal(expected, Bytes.Format(value));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
