using System.Collections.Generic;

namespace magisterDiplom.Utils
{
    public interface ILogger
    {

        void Print(string message);
        void Print(string message, int[] array);
        void Print(string message, List<List<int>> matrix);

    }
}
