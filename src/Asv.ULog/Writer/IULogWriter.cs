using System.Buffers;
using System.Runtime.CompilerServices;

namespace Asv.ULog;

public interface IULogWriter : IDisposable
{
    string SourceName { get; }
    IULogWriter AppendHeader(ULogFileHeaderToken header, ULogFlagBitsMessageToken flags);
    IULogWriter AppendDefinition(IULogDefinitionToken definitionToken);
    IULogWriter AppendData(IULogDataToken dataToken);
}

