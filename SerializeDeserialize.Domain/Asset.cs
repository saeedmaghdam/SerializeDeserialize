namespace SerializeDeserialize.Domain
{
    public class Asset : INode
    {
        public string SymbolId { get; set; }
        public decimal Quantity { get; set; }

        public List<INode> Documents { get; set; }

        public Asset()
        {
            Documents = new List<INode>();
        }
    }
}
