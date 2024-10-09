namespace Asv.ULog.ULogFile.Processors;

public class TokenProcessor
{
    private readonly Dictionary<ULogToken, ITokenHandler> _handlers;
    private readonly ProcessorContext _context;

    public TokenProcessor(Dictionary<ULogToken, ITokenHandler> handlers, ProcessorContext context)
    {
        _handlers = handlers;
        _context = context;
    }

    public TokenPlaceFlags Process(IULogToken token)
    {
        if (_handlers.TryGetValue(token.TokenType, out var handler))
        {
            handler.Handle(token, _context);
            return token.TokenSection;
        }
        else
        {
            return _context.File.ReadState; // while not all tokens are handled
            throw new UnknownTokenException();
        }
    }
}