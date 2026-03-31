using magisterDiplom.Model;
using System.Collections.Generic;

namespace magisterDiplom.Fabric
{
    /// <summary>
    /// Абстрактный класс содержащий общие функции для расписания с ПТО, наследуемый от класса Schedule
    /// </summary>
    public abstract class PreMSchedule : Schedule
    {

        /// <summary>
        /// Конфигурационная структура содержащая информацию о ПТО
        /// </summary>
        protected PreMConfig preMConfig;

        /// <summary>
        /// Матрица порядка ПТО приборов
        /// </summary>
        protected List<List<int>> matrixY;

        /// <summary>
        /// Матрица моментов времени окончания ПТО приборов
        /// </summary>
        protected List<List<PreMSet>> matrixTPM;

        /// <summary>
        /// Возвращает матрицу ПТО приборов
        /// </summary>
        /// <returns>Матрица ПТО приборов</returns>
        public abstract List<List<int>> GetMatrixY();

        /// <summary>
        /// Возвращает матрицу моментов времени окончания ПТО приборов
        /// </summary>
        /// <returns>Матрица моментов времени окончания ПТО приборов</returns>
        public abstract List<List<int>> GetMatrixTPM();
    }
}