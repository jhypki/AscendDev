namespace AscendDev.Core.Interfaces.Data;

public interface IConnectionManager<out T> where T : class
{
    T GetConnection();
}