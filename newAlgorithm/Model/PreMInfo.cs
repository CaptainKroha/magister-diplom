using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace magisterDiplom.Model
{

    /// <summary>
    /// Данная структура содержит информацию о количестве ПЗ,
    /// которые реализуются на заданном интервале времени и
    /// количестве заданий в последнем ПЗ выполняемых после заданного интервала времени
    /// </summary>
    public struct PreMInfo
    {

        /// <summary>
        /// Данная переменная содержит информацию о количестве ПЗ выполняемых на заданном интервале времени
        /// </summary>
        public int BatchCount { get; set; }

        /// <summary>
        /// Данная переменная содержит информацию о количестве заданиях выполняемых после заданного интервала времени
        /// </summary>
        public int JobCount { get; set; }

        /// <summary>
        /// Данный конструктор выполняет создание экземпляра структуру
        /// </summary>
        /// <param name="BatchCount">Количество ПЗ выполняемых на заданном интервале времени</param>
        /// <param name="JobCount">Количество заданий выполняемых после заданного интервала времени</param>
        public PreMInfo(int BatchCount, int JobCount)
        {
            this.BatchCount = BatchCount;
            this.JobCount = JobCount;
        }
    }
}
