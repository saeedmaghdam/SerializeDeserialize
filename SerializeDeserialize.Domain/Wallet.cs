namespace SerializeDeserialize.Domain
{
    public class Wallet : INode
    {
        public string Name { get; set; }
        public decimal Value { get; set; }

        public List<INode> Documents { get; set; }

        public Wallet()
        {
            Documents = new List<INode>();
        }
    }
}
