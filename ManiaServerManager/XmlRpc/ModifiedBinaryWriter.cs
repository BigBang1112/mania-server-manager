using System.Text;

namespace ManiaServerManager.XmlRpc;

internal sealed class ModifiedBinaryWriter : BinaryWriter
{
    public ModifiedBinaryWriter(Stream output) : base(output)
    {
    }

    public ModifiedBinaryWriter(Stream output, Encoding encoding) : base(output, encoding)
    {
    }

    public ModifiedBinaryWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen)
    {
    }
}
