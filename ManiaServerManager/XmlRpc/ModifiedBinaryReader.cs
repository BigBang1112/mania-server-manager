using System.Text;

namespace ManiaServerManager.XmlRpc;

internal sealed class ModifiedBinaryReader : BinaryReader
{
    public ModifiedBinaryReader(Stream input) : base(input)
    {
    }

    public ModifiedBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
    {
    }

    public ModifiedBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
    {
    }
}
