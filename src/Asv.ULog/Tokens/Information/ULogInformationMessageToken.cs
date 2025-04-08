namespace Asv.ULog.Information;

/// <summary>
/// 'I': Information Message
///
/// The Information message defines a dictionary type definition key : value pair for any information,
/// including but not limited to Hardware version, Software version, Build toolchain for the software, etc.
/// </summary>
public class ULogInformationMessageToken : ULogKeyAndValueTokenBase, IEquatable<ULogInformationMessageToken>, IULogDefinitionToken, IULogDataToken
{
    #region Static

    public static ULogToken Type => ULogToken.Information;
    public const string Name = "Information";
    public const byte TokenId = (byte)'I';

    #endregion

    public override string TokenName => Name;
    public override ULogToken TokenType => Type;
    public override UTokenPlaceFlags TokenSection => UTokenPlaceFlags.Definition | UTokenPlaceFlags.Data;
    public bool Equals(ULogInformationMessageToken? other) => base.Equals(other);

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogInformationMessageToken)obj);
    }

    public override int GetHashCode() => base.GetHashCode();
}