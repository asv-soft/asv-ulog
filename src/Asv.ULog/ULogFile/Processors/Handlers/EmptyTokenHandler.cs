namespace Asv.ULog.ULogFile.Processors;

public class EmptyTokenHandler : ITokenHandler
{
    public void Handle(IULogToken token, ProcessorContext context) { }
}