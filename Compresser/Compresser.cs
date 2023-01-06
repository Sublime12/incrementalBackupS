
using System.IO.Pipes;

namespace Compresser;

class Compresser
{

    /// <summary>
    /// Create the stream
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public NamedPipeServerStream CreateNamePipeServer(string name)
    {
        var namePipe = new NamedPipeServerStream(name, PipeDirection.In);

        return namePipe;
    }

    public void Compress()
    {

    }
}

