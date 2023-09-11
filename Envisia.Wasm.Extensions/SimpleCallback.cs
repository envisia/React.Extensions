namespace Envisia.Wasm.Extensions;

public class SimpleCallback : TaskResult
{
    public void Success(String @result)
    {
        Console.WriteLine("Result: " + @result);
    }
    public void Error(String @result)
    {
        Console.WriteLine("Error: " + @result);
    }
}