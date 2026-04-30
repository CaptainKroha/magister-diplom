using System.Collections.Generic;

namespace magisterDiplom.Utils
{
    public interface ILogger
    {

        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);

        void Print(string message);
        void Print(string message, int[] array);
        void Print(string message, List<List<int>> matrix);

    }
}
