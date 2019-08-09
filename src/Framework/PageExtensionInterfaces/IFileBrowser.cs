using System.IO;
using System.Threading.Tasks;

namespace Framework.Core
{
    public interface IFileBrowser
    {
        Task<string> SelectDocFile();
        Stream OpenFile(string fileName);
    }
}
