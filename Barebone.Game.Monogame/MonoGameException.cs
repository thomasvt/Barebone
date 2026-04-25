namespace Barebone.Game.Monogame
{
    public class MonoGameException : Exception
    {
        public MonoGameException(string message) : base(message) { }
        public MonoGameException(string message, Exception inner) : base(message, inner) { }
    }
}
