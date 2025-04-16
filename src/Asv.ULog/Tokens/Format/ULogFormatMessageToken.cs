using System.Buffers;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Asv.Common;
using Asv.IO;

namespace Asv.ULog;

/// <summary>
/// 'F': Format Message
///
/// Format message defines a single message name and its inner fields in a single string.
/// </summary>
public partial class ULogFormatMessageToken : IULogDefinitionToken, IEquatable<ULogFormatMessageToken>
{
    #region Static

    public const char FieldSeparator = ';';
    public static readonly int FieldSeparatorByteSize;
    public const char MessageAndFieldsSeparator = ':';
    public static readonly int MessageAndFieldsSeparatorByteSize;

    private const string FixedMessageNamePattern = @"[a-zA-Z0-9_\-\/]+";

    [GeneratedRegex(FixedMessageNamePattern, RegexOptions.Compiled)]
    private static partial Regex GetMessageNameRegex();

    public static readonly Regex MessageNameRegex = GetMessageNameRegex();

    public static void CheckMessageName(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty) throw new ULogException("ULog message name is empty.");
        if (MessageNameRegex.IsMatch(value) == false)
            throw new ULogException(
                $"Invalid ULog message name. Should be {FixedMessageNamePattern}. Origin value: '{value.ToString()}'");
    }

    static ULogFormatMessageToken()
    {
        var temp = FieldSeparator;
        FieldSeparatorByteSize = ULog.Encoding.GetByteCount(new ReadOnlySpan<char>(ref temp));
        temp = MessageAndFieldsSeparator;
        MessageAndFieldsSeparatorByteSize = ULog.Encoding.GetByteCount(new ReadOnlySpan<char>(ref temp));
    }

    public static ULogToken Type => ULogToken.Format;
    public const string Name = "Format";
    public const byte TokenId = (byte)'F';

    #endregion


    public string TokenName => Name;
    public ULogToken TokenType => Type;
    public UTokenPlaceFlags TokenSection => UTokenPlaceFlags.Definition;

    private string _messageName = null!;

    /// <summary>
    /// Message name
    /// </summary>
    public string MessageName
    {
        get => _messageName;
        set
        {
            CheckMessageName(value);
            _messageName = value;
        }
    }

    /// <summary>
    /// Message fields
    /// </summary>
    public IList<ULogTypeAndNameDefinition> Fields { get; } = [];

    public void Deserialize(ref ReadOnlySpan<char> input)
    {
        var colonIndex = input.IndexOf(MessageAndFieldsSeparator);
        if (colonIndex == -1)
            throw new ULogException(
                $"Invalid format message for token {Type:G}: '{MessageAndFieldsSeparator}' not found. Origin string: {input.ToString()}");
        var messageNameSpan = input[..colonIndex];
        MessageName = messageNameSpan.ToString();
        input = input[(colonIndex + 1)..];
        while (!input.IsEmpty)
        {
            var semicolonIndex = input.IndexOf(FieldSeparator);
            if (semicolonIndex == -1)
                throw new ULogException(
                    $"Invalid format message for token {Type:G}: '{FieldSeparator}' not found. Origin string: {input.ToString()}");

            var field = input[..semicolonIndex];
            var newItem = new ULogTypeAndNameDefinition();
            newItem.Deserialize(ref field);
            Debug.Assert(field.Length == 0);
            Fields.Add(newItem);
            input = input[(semicolonIndex + 1)..];
        }
    }
    
    public void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        var charSize = ULog.Encoding.GetCharCount(buffer);
        var charBuffer = ArrayPool<char>.Shared.Rent(charSize);
        try
        {
            var writeSpan = new Span<char>(charBuffer, 0, charSize);
            var size = ULog.Encoding.GetChars(buffer,writeSpan);
            Debug.Assert(charSize == size);
            var readSpan = new ReadOnlySpan<char>(charBuffer, 0, charSize);
            Deserialize(ref readSpan);
            Debug.Assert(readSpan.Length == 0);
            buffer = buffer[charSize..];
        }
        finally
        {
            ArrayPool<char>.Shared.Return(charBuffer);
        }
    }

    public void Serialize(ref Span<byte> buffer)
    {
        // we need to serialize message name and fields e.g. message_name:field1;field2;field3;
        MessageName.CopyTo(ref buffer, ULog.Encoding);
        var temp = MessageAndFieldsSeparator;
        var write = ULog.Encoding.GetBytes(new ReadOnlySpan<char>(ref temp), buffer);
        buffer = buffer[write..];
        foreach (var field in Fields)
        {
            field.Serialize(ref buffer);
            temp = FieldSeparator;
            write = ULog.Encoding.GetBytes(new ReadOnlySpan<char>(ref temp), buffer);
            buffer = buffer[write..];
        }
    }

    public int GetByteSize()
    {
        return ULog.Encoding.GetByteCount(MessageName) + MessageAndFieldsSeparatorByteSize +
               Fields.Sum(x => x.GetByteSize() + FieldSeparatorByteSize);
    }

    public bool Equals(ULogFormatMessageToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return MessageName == other.MessageName && Fields.SequenceEqual(other.Fields);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogFormatMessageToken)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MessageName, Fields);
    }
}