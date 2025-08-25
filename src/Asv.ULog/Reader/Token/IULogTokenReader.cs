using System;
using System.IO;

namespace Asv.ULog;

public interface IULogTokenReader
{
    bool TryRead(out IULogToken? token);
}