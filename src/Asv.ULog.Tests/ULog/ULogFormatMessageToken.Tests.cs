using Asv.ULog;

namespace Asv.ULog.Tests;

public class ULogFormatMessageTokenTests
{
    [Theory]
    [InlineData("Test:char[5] TestChar;", "Test", "TestChar", "char", 5)]
    [InlineData("Data:int32 Value;", "Data", "Value", "int32", 0)]
    public void Deserialize_Success(string input, string expectedMessageName, string expectedFieldName, string expectedTypeName, int expectedArraySize)
    {
        // Arrange
        var buffer = ULogManager.Encoding.GetBytes(input);
        var readOnlySpan = new ReadOnlySpan<byte>(buffer);
        var token = new ULogFormatMessageToken();

        // Act
        token.Deserialize(ref readOnlySpan);

        // Assert
        Assert.Equal(expectedMessageName, token.MessageName);
        Assert.Single(token.Fields);
        Assert.Equal(expectedFieldName, token.Fields[0].Name);
        Assert.Equal(expectedTypeName, token.Fields[0].Type.TypeName);
        Assert.Equal(expectedArraySize, token.Fields[0].Type.ArraySize);
    }

    [Theory]
    [InlineData("Test char[5] TestChar;")] // Invalid format (missing ':')
    [InlineData("NoSeparatorField")] // Missing field separator
    public void Deserialize_InvalidFormat_ThrowsException(string input)
    {
        // Arrange
        var buffer = ULogManager.Encoding.GetBytes(input);
        var token = new ULogFormatMessageToken();
    
        // Делаем копию буфера, чтобы использовать внутри лямбды
        var bufferCopy = buffer.ToArray();

        // Act & Assert
        Assert.Throws<ULogException>(() =>
        {
            var spanCopy = new ReadOnlySpan<byte>(bufferCopy);
            token.Deserialize(ref spanCopy);
        });
    }
    
    [Theory]
    [InlineData("Test:char[5] TestChar;", "Test", "TestChar", "char", 5)]
    [InlineData("Data:int32 Value;", "Data", "Value", "int32", 0)]
    public void Serialize_Success(string expectedOutput, string messageName, string fieldName, string typeName, int arraySize)
    {
        // Arrange
        var token = new ULogFormatMessageToken
        {
            MessageName = messageName,
            Fields =
            {
                new ULogTypeAndNameDefinition
                {
                    Name = fieldName,
                    Type = new ULogTypeDefinition
                    {
                        BaseType = arraySize > 0 ? ULogType.Char : ULogType.Int32,
                        ArraySize = arraySize,
                        TypeName = typeName
                    }
                }
            }
        };

        var buffer = new byte[token.GetByteSize()];
        var span = new Span<byte>(buffer);

        // Act
        token.Serialize(ref span);

        var serializedString = ULogManager.Encoding.GetString(buffer);

        // Assert
        Assert.Equal(expectedOutput, serializedString);
    }
    
    [Theory]
    [InlineData("Test", "TestChar", "char", 5, 22)] // "Test:char[5] TestChar;"
    [InlineData("Data", "Value", "int32", 0, 17)]   // "Data:int32 Value;"
    public void GetByteSize_Success(string messageName, string fieldName, string typeName, int arraySize, int expectedByteSize)
    {
        // Arrange
        var token = new ULogFormatMessageToken
        {
            MessageName = messageName,
            Fields =
            {
                new ULogTypeAndNameDefinition
                {
                    Name = fieldName,
                    Type = new ULogTypeDefinition
                    {
                        BaseType = arraySize > 0 ? ULogType.Char : ULogType.Int32,
                        ArraySize = arraySize,
                        TypeName = typeName
                    }
                }
            }
        };

        // Act
        var actualByteSize = token.GetByteSize();

        // Assert
        Assert.Equal(expectedByteSize, actualByteSize);
    }
}