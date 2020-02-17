namespace Origam.DA.Service
{
    public class XmlLoadError
    {
        public ErrType Type { get;}
        public readonly string Message;

        public XmlLoadError(ErrType type, string message)
        {
            Message = message;
            Type = type;
        }

        public XmlLoadError(string message)
        {
            Message = message;
            Type = ErrType.XmlGeneralError;
        }
    }
    
    public enum ErrType
    {
        XmlVersionIsOlderThanCurrent,
        XmlGeneralError
    }
}