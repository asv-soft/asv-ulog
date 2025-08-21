using System.Buffers;
using Asv.IO;

namespace Asv.ULog;

public enum ULogType
{
    Int8,
    UInt8,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Float,
    Double,
    Bool,
    Char,
    ReferenceType
}

/// <summary>
/// Represents a ULog type definition.
///
/// e.g., float[5], int8_t, etc.
/// </summary>
public class ULogTypeDefinition : ISizedSpanSerializable, IEquatable<ULogTypeDefinition>
{
    #region Static

    public const char ArrayStart = '[';
    public static readonly int ArrayStartByteSize;
    public const char ArrayEnd = ']';
    public static readonly int ArrayEndByteSize;

    public const string Int8TypeName = "int8_t";
    public static ULogTypeDefinition Int8Type = new()
    {
        BaseType = ULogType.Int8,
        TypeName = Int8TypeName,
        ArraySize = 0,
    };
    public const string UInt8TypeName = "uint8_t";
    public static ULogTypeDefinition UInt8Type = new()
    {
        BaseType = ULogType.UInt8,
        TypeName = UInt8TypeName,
        ArraySize = 0,
    };
    public const string Int16TypeName = "int16_t";
    public static ULogTypeDefinition Int16Type = new()
    {
        BaseType = ULogType.Int16,
        TypeName = Int16TypeName,
        ArraySize = 0,
    };
    public const string UInt16TypeName = "uint16_t";
    public static ULogTypeDefinition UInt16Type = new()
    {
        BaseType = ULogType.UInt16,
        TypeName = UInt16TypeName,
        ArraySize = 0,
    };
    public const string Int32TypeName = "int32_t";
    public static ULogTypeDefinition Int32Type = new()
    {
        BaseType = ULogType.Int32,
        TypeName = Int32TypeName,
        ArraySize = 0,
    };
    public const string UInt32TypeName = "uint32_t";
    public static ULogTypeDefinition UInt32Type = new()
    {
        BaseType = ULogType.UInt32,
        TypeName = UInt32TypeName,
        ArraySize = 0,
    };
    public const string Int64TypeName = "int64_t";
    public static ULogTypeDefinition Int64Type = new()
    {
        BaseType = ULogType.Int64,
        TypeName = Int64TypeName,
        ArraySize = 0,
    };
    public const string UInt64TypeName = "uint64_t";
    public static ULogTypeDefinition UInt64Type = new()
    {
        BaseType = ULogType.UInt64,
        TypeName = UInt64TypeName,
        ArraySize = 0,
    };
    public const string FloatTypeName = "float";
    public static ULogTypeDefinition FloatType = new()
    {
        BaseType = ULogType.Float,
        TypeName = FloatTypeName,
        ArraySize = 0,
    };
    public const string DoubleTypeName = "double";
    public static ULogTypeDefinition DoubleType = new()
    {
        BaseType = ULogType.Double,
        TypeName = DoubleTypeName,
        ArraySize = 0,
    };
    public const string BoolTypeName = "bool";
    public static ULogTypeDefinition BoolType = new()
    {
        BaseType = ULogType.Bool,
        TypeName = BoolTypeName,
        ArraySize = 0,
    };
    public const string CharTypeName = "char";
    public static ULogTypeDefinition CharType = new()
    {
        BaseType = ULogType.Char,
        TypeName = CharTypeName,
        ArraySize = 0,
    };

    static ULogTypeDefinition()
    {
        var temp = ArrayStart;
        ArrayStartByteSize = ULogManager.Encoding.GetByteCount(new ReadOnlySpan<char>(ref temp));
        temp = ArrayEnd;
        ArrayEndByteSize = ULogManager.Encoding.GetByteCount(new ReadOnlySpan<char>(ref temp));
    }

    #endregion

    private int _arraySize;
    private string _typeName = null!;
    private ULogType _baseType;

    public string TypeName
    {
        get => _typeName;
        set => _typeName = value;
    }

    public ULogType BaseType
    {
        get => _baseType;
        set => _baseType = value;
    }

    public bool IsArray => _arraySize > 0;

    public int ArraySize
    {
        get => _arraySize;
        set => _arraySize = value;
    }

    public void Deserialize(ReadOnlySpan<char> buffer)
    {
        _arraySize = 0;
        // Trim any leading or trailing whitespace
        buffer = buffer.Trim();
        // Check for array format (e.g., float[5])
        var openBracketIndex = buffer.IndexOf(ArrayStart);
        var closeBracketIndex = buffer.IndexOf(ArrayEnd);
        if (openBracketIndex != -1 && closeBracketIndex != -1)
        {
            // Parse array size
            var arraySizeSpan = buffer.Slice(openBracketIndex + 1, closeBracketIndex - openBracketIndex - 1);
            if (!int.TryParse(arraySizeSpan, out _arraySize)) throw new FormatException("Invalid array size format.");

            // Extract the type name without the array size
            _typeName = buffer[..openBracketIndex].Trim().ToString();
        }
        else
        {
            // If not an array, use the entire string as the type name
            _typeName = buffer.Trim().ToString();
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(_typeName);
        _baseType = _typeName switch
        {
            Int8TypeName => ULogType.Int8,
            UInt8TypeName => ULogType.UInt8,
            Int16TypeName => ULogType.Int16,
            UInt16TypeName => ULogType.UInt16,
            Int32TypeName => ULogType.Int32,
            UInt32TypeName => ULogType.UInt32,
            Int64TypeName => ULogType.Int64,
            UInt64TypeName => ULogType.UInt64,
            FloatTypeName => ULogType.Float,
            DoubleTypeName => ULogType.Double,
            BoolTypeName => ULogType.Bool,
            CharTypeName => ULogType.Char,
            _ => ULogType.ReferenceType
        };
    }

    public void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        var charSize = ULogManager.Encoding.GetCharCount(buffer);
        var charBuffer = ArrayPool<char>.Shared.Rent(charSize);
        try
        {
            var rawString = new ReadOnlySpan<char>(charBuffer, 0, charSize);
            ULogManager.Encoding.GetChars(buffer, charBuffer);
            Deserialize(rawString);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(charBuffer);
        }
    }

    public void Serialize(ref Span<byte> buffer)
    {
        if (IsArray)
        {
            _typeName.CopyTo(ref buffer, ULogManager.Encoding);
            var temp = ArrayStart;
            var written = ULogManager.Encoding.GetBytes(new ReadOnlySpan<char>(ref temp), buffer);
            buffer = buffer[written..];
            _arraySize.ToString().CopyTo(ref buffer, ULogManager.Encoding);
            temp = ArrayEnd;
            written = ULogManager.Encoding.GetBytes(new ReadOnlySpan<char>(ref temp), buffer);
            buffer = buffer[written..];
        }
        else
        {
            _typeName.CopyTo(ref buffer, ULogManager.Encoding);
        }
    }

    public int GetByteSize()
    {
        return IsArray
            ? ULogManager.Encoding.GetByteCount(_typeName) +
              ArrayStartByteSize + ULogManager.Encoding.GetByteCount(_arraySize.ToString()) + ArrayEndByteSize
            : ULogManager.Encoding.GetByteCount(_typeName);
    }

    public bool Equals(ULogTypeDefinition? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ArraySize == other.ArraySize && TypeName == other.TypeName && BaseType == other.BaseType;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ULogTypeDefinition)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ArraySize, TypeName, (int)BaseType);
    }

    public ULogTypeDefinition Clone()
    {
        return new ULogTypeDefinition
        {
            TypeName = TypeName,
            BaseType = BaseType,
            ArraySize = ArraySize
        };
    }
    
    public override string ToString()
    {
        var size = GetByteSize();
        if (size <= 0) return string.Empty;

        var rented = ArrayPool<byte>.Shared.Rent(size);
        try
        {
            var span = new Span<byte>(rented, 0, size);
            var before = span.Length;
            Serialize(ref span);
            var written = before - span.Length;

            return ULogManager.Encoding.GetString(rented, 0, written);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    
    
}