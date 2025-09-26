namespace Repositorios
{
    public interface IRepositorio<T>
    {
        //crud basico
        List<T> ObtenerTodos(); //no es necesario
        bool Crear(T entity); // no hay registros en la app
        bool Actualizar(T entity); // si es necesario
        bool EliminadoLogico(int id); //no es necesario
        T BuscarPorId(int id); // tampoco es necesario
    }

    

}