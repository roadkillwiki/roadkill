using System.Collections.Generic;

// https://github.com/Workshop2/LocalDbApi
namespace LocalDbApi
{
    public interface IDbCommunication
    {
        void Execute(string arguments);
        string ExecuteString(string arguments);
        IEnumerable<string> ExecuteList(string arguments);
    }
}