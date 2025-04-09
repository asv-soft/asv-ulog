using Asv.IO;

namespace Asv.ULog;

public enum ULogToken : byte
{
    Unknown,
    FileHeader,
    FlagBits = (byte)'B',
    Format = (byte)'F',
    Information = (byte)'I',
    MultiInformation = (byte)'M',
    Parameter = (byte)'P',
    DefaultParameter = (byte)'Q',
    Unsubscription = (byte)'R',
    Subscription = (byte)'A',
    LoggedData = (byte)'D',
    LoggedString = (byte)'L',
    Synchronization = (byte)'S',
    TaggedLoggedString = (byte)'C',
    Dropout = (byte)'O',
}
public interface IULogToken : ISizedSpanSerializable
{
    string TokenName { get; }
    ULogToken TokenType { get; }
    UTokenPlaceFlags TokenSection { get; }
}

public interface IULogDefinitionToken : IULogToken;

public interface IULogDataToken : IULogToken;

/// <summary>
/// ULog files have the following three sections:
/// 
/// ----------------------
/// |       Header       |
/// ----------------------
/// |    Definitions     |
/// ----------------------
/// |        Data        |
/// ----------------------
/// </summary>
[Flags]
public enum UTokenPlaceFlags
{
    None = 0,

    /// <summary>
    /// Token can be placed only in header section
    /// </summary>
    Header = 0b0000_0001,

    /// <summary>
    /// Token can be placed only in definition section
    /// </summary>
    Definition = 0b0000_0010,

    /// <summary>
    /// Token can be placed only in data section
    /// </summary>
    Data = 0b0000_0100,

    /// <summary>
    /// Token can be placed in header and definition sections
    /// </summary>
    DefinitionAndData = Definition | Data
}