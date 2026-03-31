using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace magisterDiplom.Model
{

    /// <summary>
    /// Данная структура описывает позицию ПЗ после которого выполняется ПТО и время окончания данного ПТО
    /// </summary>
    public struct PreMSet
    {

        /// <summary>
        /// Данная перемення описывает индекс ПЗ после которого выполняется ПТО
        /// </summary>
        public int BatchIndex {  get; set; }

        /// <summary>
        /// Данная переменная описывает момент времени окончания ПТО после ПЗ на позиции batchIndex
        /// </summary>
        public int TimePreM {  get; set; }

        /// <summary>
        /// Данная перменная описывает конструктор данной структуру
        /// </summary>
        /// <param name="BatchIndex">индекс ПЗ после которого выполняется ПТО</param>
        /// <param name="TimePreM">момент времени окончания ПТО после ПЗ на позиции batchIndex</param>
        public PreMSet(int BatchIndex, int TimePreM)
        {
            this.BatchIndex = BatchIndex;
            this.TimePreM = TimePreM;
        }
    }
}
