using System.Buffers;
using System.Diagnostics;
using Asv.ULog;
using Xunit.Abstractions;

namespace Asv.Ulog.Tests.ULogValue;

public class ULogValueTest(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    [Fact]
    public void Read_data_From_File_And_Parse()
    {
        var data = new ReadOnlySequence<byte>(TestData.ulog_log_small);
        var rdr = new SequenceReader<byte>(data);
        var reader = ULog.ULog.CreateReader();
        var format = new Dictionary<string, ULogFormatMessageToken>();
        var subscriptions = new Dictionary<ushort, ULogSubscriptionMessageToken>();
        var logged = new List<ULogLoggedDataMessageToken>();
        
        while (reader.TryRead(ref rdr, out var token))
        {
            Debug.Assert(token != null, nameof(token) + " != null");
            switch (token.TokenType)
            {
               
                case ULogToken.Format:
                    var formatToken = (ULogFormatMessageToken) token;
                    format.Add(formatToken.MessageName, formatToken);
                    break;
                
                case ULogToken.Subscription:
                    var subscriptionToken = (ULogSubscriptionMessageToken) token;
                    subscriptions.Add(subscriptionToken.MessageId, subscriptionToken);
                    break;
                case ULogToken.LoggedData:
                    var loggedToken = (ULogLoggedDataMessageToken) token;
                    logged.Add(loggedToken);
                    break;
                case ULogToken.Unknown:
                case ULogToken.FileHeader:
                case ULogToken.FlagBits:
                case ULogToken.Information:
                case ULogToken.MultiInformation:
                case ULogToken.Parameter:
                case ULogToken.DefaultParameter:
                case ULogToken.Unsubscription:
                case ULogToken.LoggedString:
                case ULogToken.Synchronization:
                case ULogToken.TaggedLoggedString:
                case ULogToken.Dropout:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        var selectedItem = logged.Skip(3).First();
        var value = ULog.ULog.Create(selectedItem, format, subscriptions, out var messageName);
        foreach (var item in logged.Where(x=>x.MessageId == selectedItem.MessageId))
        {
            var span = new ReadOnlySpan<byte>(item.Data);
            value.Deserialize(ref span);
            Assert.Equal(0, span.Length);
            _output.WriteLine($"{messageName}:{value}");
        }
        
    }
}