namespace Eviden.VirtualGrocer.Web.Server.Skills.History
{
    public interface ITokenCounter
    {
        int CountTokens(string message);
    }
}
