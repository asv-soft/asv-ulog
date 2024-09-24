namespace Asv.ULog.ULogFile.Processors;

public interface ITokenHandler
{
    public void Handle(IULogToken token, ProcessorContext context);
}