namespace Banks.Utils;

public interface IObserver
{
    void HandleUpdate(string type, string value);
}