using Asv.IO;

namespace Asv.ULog;

public interface IULogWriter : IDisposable, IAsyncDisposable
{
    IDefinitionSection Definition { get; }
    IDataSection Data { get; }
    
    public interface IDefinitionSection
    {
        void DefineParameter(string name, int value, ULogParameterDefaultTypes type);
        void DefineParameter(string name, float value, ULogParameterDefaultTypes type);
        void DefineFormat(ULogFormatMessageToken token);
    }
    
    public interface IDataSection
    {
        void WriteParameter(string name, int value);
        void WriteParameter(string name, float value);
        void WriteSubscription(string messageName, byte multiId);
        void WriteData(string messageName, byte multiId, ISizedSpanSerializable data);
    }
}



