namespace Service.Application.Exceptions
{
    public class NotFoundExeption
    {
        public class NotFoundException : Exception
        {

            public NotFoundException(string name, object key)
                : base($"Entity \"{name}\" ({key}) not found") { }

        }
    }
}
