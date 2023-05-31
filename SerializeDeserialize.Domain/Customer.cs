namespace SerializeDeserialize.Domain
{
    public class Customer : INode
    {
        public string Name { get; set; }
        public string Family { get; set; }

        public List<INode> Documents { get; set; }

        public Customer()
        {
            Documents = new List<INode>();
        }
    }
}
