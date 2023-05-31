namespace SerializeDeserialize.Domain
{
    public class Orderbook : INode
    {
        public decimal Order1BidQuantity { get; set; }
        public decimal Order1BidPrice { get; set; }
        public decimal Order1AskQuantity { get; set; }
        public decimal Order1AskPrice { get; set; }

        public List<INode> Documents { get; set; }

        public Orderbook()
        {
            Documents = new List<INode>();
        }
    }
}
