using System.Globalization;
using Asv.IO;
using Newtonsoft.Json.Linq;

namespace Asv.ULog;

public enum UValueType
{
    Property,
    Array,
    Object,
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
}

public abstract class ULogValue:ISizedSpanSerializable,ICloneable
{
    #region Static type validation

    private static readonly UValueType[] BooleanTypes = 
        [UValueType.Bool, UValueType.UInt8, UValueType.UInt16, UValueType.UInt32, UValueType.UInt64, UValueType.Int8, UValueType.Int16, UValueType.Int32, UValueType.Int64, UValueType.Float, UValueType.Double];
    private static readonly UValueType[] IntegerTypes = 
        [UValueType.UInt8, UValueType.UInt16, UValueType.UInt32, UValueType.UInt64, UValueType.Int8, UValueType.Int16, UValueType.Int32, UValueType.Int64];

    private static readonly UValueType[] ReadTypesTypes = [UValueType.Double, UValueType.Float];
    
    
    private static ULogSimple? EnsureSimple(ULogValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value is ULogProperty property)
        {
            value = property.Value;
        }

        var v = value as ULogSimple;

        return v;
    }
    
    private static string GetType(ULogValue token)
    {
        if (token is ULogProperty p)
        {
            token = p.Value;
        }
        return token.Type.ToString();
    }
    
    private static bool ValidateSimpleValue(ULogValue o, UValueType[] validTypes, bool nullable)
    {
        return Array.IndexOf(validTypes, o.Type) != -1;
    }

    #endregion
    
    internal ULogValue()
    {
        
    }
    
    #region Cast from operators
    
    public static explicit operator bool(ULogValue value)
    {
        var v = EnsureSimple(value);
        if (v == null || !ValidateSimpleValue(v, BooleanTypes, false))
        {
            throw new ArgumentException($"Can not convert {GetType(value)} to Boolean.");
        }
        return Convert.ToBoolean(v.GetValue(), CultureInfo.InvariantCulture);
    }
    
    public static explicit operator sbyte(ULogValue value)
    {
        var v = EnsureSimple(value);
        if (v == null || !ValidateSimpleValue(v, IntegerTypes, false))
        {
            throw new ArgumentException($"Can not convert {GetType(value)} to SByte.");
        }
        return Convert.ToSByte(v.GetValue(), CultureInfo.InvariantCulture);
    }
    
    public static explicit operator byte(ULogValue value)
    {
        var v = EnsureSimple(value);
        if (v == null || !ValidateSimpleValue(v, IntegerTypes, false))
        {
            throw new ArgumentException($"Can not convert {GetType(value)} to Byte.");
        }
        return Convert.ToByte(v.GetValue(), CultureInfo.InvariantCulture);
    }
    
    public static explicit operator short(ULogValue value)
    {
        var v = EnsureSimple(value);
        if (v == null || !ValidateSimpleValue(v, IntegerTypes, false))
        {
            throw new ArgumentException($"Can not convert {GetType(value)} to Int16.");
        }
        return Convert.ToInt16(v.GetValue(), CultureInfo.InvariantCulture);
    }
    
    public static explicit operator ushort(ULogValue value)
    {
        var v = EnsureSimple(value);
        if (v == null || !ValidateSimpleValue(v, IntegerTypes, false))
        {
            throw new ArgumentException($"Can not convert {GetType(value)} to UInt16.");
        }
        return Convert.ToUInt16(v.GetValue(), CultureInfo.InvariantCulture);
    }
    
    public static explicit operator int(ULogValue value)
    {
        var v = EnsureSimple(value);
        if (v == null || !ValidateSimpleValue(v, IntegerTypes, false))
        {
            throw new ArgumentException($"Can not convert {GetType(value)} to Int32.");
        }
        return Convert.ToInt32(v.GetValue(), CultureInfo.InvariantCulture);
    }
    
    public static explicit operator uint(ULogValue value)
    {
        var v = EnsureSimple(value);
        if (v == null || !ValidateSimpleValue(v, IntegerTypes, false))
        {
            throw new ArgumentException($"Can not convert {GetType(value)} to UInt32.");
        }
        return Convert.ToUInt32(v.GetValue(), CultureInfo.InvariantCulture);
    }
    
    public static explicit operator long(ULogValue value)
    {
        var v = EnsureSimple(value);
        if (v == null || !ValidateSimpleValue(v, IntegerTypes, false))
        {
            throw new ArgumentException($"Can not convert {GetType(value)} to Int64.");
        }
        return Convert.ToInt64(v.GetValue(), CultureInfo.InvariantCulture);
    }
    
    public static explicit operator ulong(ULogValue value)
    {
        var v = EnsureSimple(value);
        if (v == null || !ValidateSimpleValue(v, IntegerTypes, false))
        {
            throw new ArgumentException($"Can not convert {GetType(value)} to ULogUInt64.");
        }
        return Convert.ToUInt64(v.GetValue(), CultureInfo.InvariantCulture);
    }
    
    public static explicit operator float(ULogValue value)
    {
        var v = EnsureSimple(value);
        if (v == null || !ValidateSimpleValue(v, ReadTypesTypes, false))
        {
            throw new ArgumentException($"Can not convert {GetType(value)} to Single.");
        }
        return Convert.ToSingle(v.GetValue(), CultureInfo.InvariantCulture);
    }
    
    public static explicit operator double(ULogValue value)
    {
        var v = EnsureSimple(value);
        if (v == null || !ValidateSimpleValue(v, ReadTypesTypes, false))
        {
            throw new ArgumentException($"Can not convert {GetType(value)} to Double.");
        }
        return Convert.ToDouble(v.GetValue(), CultureInfo.InvariantCulture);
    }
    
    public static explicit operator char(ULogValue value)
    {
        var v = EnsureSimple(value);
        if (v == null || !ValidateSimpleValue(v, new[] {UValueType.Char}, false))
        {
            throw new ArgumentException($"Can not convert {GetType(value)} to Char.");
        }
        return Convert.ToChar(v.GetValue(), CultureInfo.InvariantCulture);
    }
    
    #endregion
    
    #region Cast to operators
    
    public static implicit operator ULogValue(bool value)
    {
        return new ULogBool(value);
    }
    
    public static implicit operator ULogValue(sbyte value)
    {
        return new ULogInt8(value);
    }
    
    public static implicit operator ULogValue(byte value)
    {
        return new ULogUInt8(value);
    }
    
    public static implicit operator ULogValue(short value)
    {
        return new ULogInt16(value);
    }
    
    public static implicit operator ULogValue(ushort value)
    {
        return new ULogUInt16(value);
    }
    
    public static implicit operator ULogValue(int value)
    {
        return new ULogInt32(value);
    }
    
    public static implicit operator ULogValue(uint value)
    {
        return new ULogUInt32(value);
    }
    
    public static implicit operator ULogValue(long value)
    {
        return new ULogInt64(value);
    }
    
    public static implicit operator ULogValue(ulong value)
    {
        return new ULogUInt64(value);
    }
    
    public static implicit operator ULogValue(float value)
    {
        return new ULogFloat(value);
    }
    
    public static implicit operator ULogValue(double value)
    {
        return new ULogDouble(value);
    }
    
    public static implicit operator ULogValue(char value)
    {
        return new ULogChar(value);
    }
    
    #endregion

    public ULogValue Root
    {
        get
        {
            var parent = Parent;
            if (parent == null)
            {
                return this;
            }
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }
            return parent;
        }
    }
    public ULogContainer? Parent { get; internal set; }
    public abstract UValueType Type { get; }
    public object Clone() => CloneToken();
    public abstract ULogValue CloneToken();
    public abstract void Deserialize(ref ReadOnlySpan<byte> buffer);
    public abstract void Serialize(ref Span<byte> buffer);
    public abstract int GetByteSize();
    
}