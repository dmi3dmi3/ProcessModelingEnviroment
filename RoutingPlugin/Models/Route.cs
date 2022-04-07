namespace RoutingPlugin.Models
{
    public class Route
    {
        public Route(Point @from, Point to, Point target)
        {
            From = @from;
            To = to;
            Target = target;
        }

        public Point From { get; set; }
        public Point To { get; set; }
        public Point Target { get; set; }
    }
}