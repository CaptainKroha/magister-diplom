using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newAlgorithm
{

    /// <summary>
    /// Описывает класс элемента расписания
    /// </summary>
    class SheduleElement
    {

        /// <summary>
        /// Количество заданий
        /// </summary>
        private int jobCount;

        /// <summary>
        /// Тип данных
        /// </summary>
        private int dataType;

        /// <summary>
        /// Список из времён
        /// </summary>
        private List<int> time;

        /// <summary>
        /// Создаём экземпляр класса SheduleElement
        /// </summary>
        /// <param name="jobCount">Количество заданий</param>
        /// <param name="dataType">Тип данных</param>
        /// <param name="time">Вектор длиной в количество заданий (jobCount) и каждый элемент вектора это время конца обработки</param>
        public SheduleElement(int jobCount, int dataType, List<int> time)
        {
            this.jobCount = jobCount;
            this.dataType = dataType;
            this.time = new List<int>(time);
        }

        /// <summary>
        /// Возвращаем количество заданий
        /// </summary>
        /// <returns>Количество заданий в партии</returns>
        public int getJobCount()
        {
            return jobCount;
        }

        /// <summary>
        /// Данная функция вовзращает тип задания
        /// </summary>
        /// <returns>Тип заданияы</returns>
        public int getType()
        {
            return dataType;
        }

        /// <summary>
        /// Данная функция вовзращает временной список
        /// </summary>
        /// <returns>Список из элементов времени</returns>
        public List<int> getTime()
        {
            return time;
        }
    }
}
