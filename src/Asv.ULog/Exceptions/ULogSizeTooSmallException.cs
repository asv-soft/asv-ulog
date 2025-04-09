namespace Asv.ULog;

public class ULogSizeTooSmallException(string section)
    : ULogException($"Size too small to read {section}");