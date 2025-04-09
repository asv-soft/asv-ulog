namespace Asv.ULog;

public sealed class ULogWrongTokenSectionException() 
    : ULogException("Token was found in a wrong section");