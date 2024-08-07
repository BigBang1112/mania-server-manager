namespace ManiaServerManager.XmlRpc;

[Serializable]
public class XmlRpcFaultException : Exception
{
    public XmlRpcFaultException() { }
    public XmlRpcFaultException(string? message) : base(message) { }
    public XmlRpcFaultException(string? message, Exception inner) : base(message, inner) { }
}
