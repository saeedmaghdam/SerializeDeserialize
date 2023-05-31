using SerializeDeserialize.Domain;

namespace SerializeDeserialize.Tests
{
    public class MicrosoftSerializerTests
    {
        [Fact]
        public void Test1()
        {
            var customer = new Customer()
            {
                Name = "Saeed",
                Family = "Aghdam",
                Documents = new List<INode>()
                {
                    new Asset
                    {
                        Quantity = 100,
                        SymbolId = "BTC-USDT",
                        Documents = new List<INode>()
                        {
                            new Orderbook
                            {
                                Order1AskPrice = 23000,
                                Order1AskQuantity = 1009,
                                Order1BidPrice = 22600,
                                Order1BidQuantity = 5000
                            }
                        }
                    },
                    new Wallet
                    {
                        Name = "Wallet1",
                        Value = 65000
                    },
                    new Wallet
                    {
                        Name = "Wallet2",
                        Value = 2000
                    }
                }
            };

            var microsoftSerializer = new MicrosoftSerializer();
            var json = microsoftSerializer.Serialize(customer);
            var deserializedCustomer = microsoftSerializer.Deserialize(json);

            Assert.True(deserializedCustomer.AreObjectsEqual(customer));
        }
    }
}