namespace Model;

public interface IKeyed<T>
{
    abstract T GetKey();
}