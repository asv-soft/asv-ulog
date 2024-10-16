using System.Diagnostics;

namespace Asv.ULog.ULogFile.Processors;

public class FlagBitsTokenHandler : ITokenHandler
{
    public void Handle(IULogToken token, ProcessorContext context)
    {
        Debug.Assert(context.File.ReadState == TokenPlaceFlags.Header);
        var flagBits = token as ULogFlagBitsMessageToken;
        context.File.Definition.FlagBits.CompatFlags = flagBits!.CompatFlags;
        context.File.Definition.FlagBits.CompatFlags = flagBits.IncompatFlags;
        context.File.Definition.FlagBits.AppendedOffsets = flagBits.AppendedOffsets;
    }
}