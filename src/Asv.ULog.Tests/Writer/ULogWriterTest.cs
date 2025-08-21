using System.IO.Compression;
using Asv.ULog;
using JetBrains.Annotations;

namespace Asv.Ulog.Tests;

[TestSubject(typeof(ULogWriter))]
public class ULogWriterTest
{

    [Fact]
    public void METHOD()
    {
        var dir = Path.Combine(Path.GetTempPath(),"ULOG_TESTS");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        var path = Path.Combine(dir, "ulog_test.ulg.gzip");
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using var zip = new GZipStream(File.Create(path), CompressionLevel.SmallestSize);
            
            using var writer = ULog.ULog.CreateWriter(zip,path);
            writer.AppendHeader(DateTime.Now);
            writer.AppendData(new ULogSubscriptionMessageToken
            {
                MultiId = 0,
                MessageId = 0,
                MessageName = null
            });
            writer.AppendDefinition(new ULogParameterMessageToken
            {
                Key = new ULogTypeAndNameDefinition
                {
                    Type = new ULogTypeDefinition
                    {
                        BaseType = ULogType.Float,
                        TypeName = "float",
                    },
                    Name = "PARAM_0000",
                },
                Value = 0.0f,
            });
            writer.AppendDefinition(new ULogFormatMessageToken
            {
                Fields =
                {
                    new ULogTypeAndNameDefinition
                    {
                        Type = new ULogTypeDefinition
                        {
                            BaseType = ULogType.Float,
                            TypeName = "float",
                        },
                        Name = "PARAM_0000",
                    }
                }
            });
            for (int i = 0; i < 1000; i++)
            {
                writer.AppendDefinition(new ULogDefaultParameterMessageToken
                {
                    Key = new ULogTypeAndNameDefinition
                    {
                        Type = new ULogTypeDefinition
                        {
                            BaseType = ULogType.Float,
                            TypeName = "float",
                        },
                        Name = $"PARAM_{i:0000}",
                    },
                    Value = 0.0f,
                    DefaultType = ULogParameterDefaultTypes.SystemWide,
                });
            }
           
        }
        finally
        {
            File.Delete(path);
        }
        
    }
}