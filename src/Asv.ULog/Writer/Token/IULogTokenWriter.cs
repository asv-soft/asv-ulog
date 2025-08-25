namespace Asv.ULog;

public interface IULogTokenWriter : IDisposable
{
    string SourceName { get; }
    IULogTokenWriter AppendHeader(ULogFileHeaderToken header, ULogFlagBitsMessageToken flags);
    IULogTokenWriter AppendDefinition(IULogDefinitionToken definitionToken);
    IULogTokenWriter AppendData(IULogDataToken dataToken);
}

