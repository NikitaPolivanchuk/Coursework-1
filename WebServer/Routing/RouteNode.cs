namespace Webserver.Routing
{
    internal class RouteNode
    {
        private readonly Dictionary<string, RouteNode> subNodes = [];
        private readonly Dictionary<Type, RouteNode> argSubNodes = [];

        public string Name { get; }

        public RouteMethod? Method { get; set; }

        public RouteNode(string name)
        {
            Name = name;
        }

        public void AddSubNode(string name, RouteNode node) => subNodes[name] = node;

        public void AddArgSubNode(Type type, RouteNode node) => argSubNodes[type] = node;

        public bool TryGetSubNode(string name, out RouteNode? node) => subNodes.TryGetValue(name, out node);

        public bool TryGetArgSubNode(Type type, out RouteNode? node) => argSubNodes.TryGetValue(type, out node);
    }
}
