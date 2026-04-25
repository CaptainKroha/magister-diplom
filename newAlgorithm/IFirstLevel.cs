using System.Collections.Generic;

namespace newAlgorithm
{
    /// <summary>
    /// Интерфейс для алгоритмов первого уровня формирования составов партий
    /// </summary>
    public interface IFirstLevel
    {
        /// <summary>
        /// Основной метод для запуска процесса генерации решения
        /// </summary>
        void GenerateSolution();

        /// <summary>
        /// Матрица составов партий требований на текущем шаге
        /// </summary>
        List<List<int>> PrimeMatrixA { get; }
    }
}
