using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace magisterDiplom.Model
{

    /// <summary>
    /// Данный класс описывает партию одного типа данных
    /// </summary>
    public class Batch
    {

        /// <summary>
        /// Данная переменная описывает тип данных партии
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Данная переменная описывает количество данных в партии
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Данный конструктор создаём партию типа данных type и размером size
        /// </summary>
        /// <param name="type">Тип данных партии</param>
        /// <param name="size">Размер партии</param>
        public Batch(int type, int size)
        {
            Type = type;
            Size = size;
        }

        /// <summary>
        /// Данная функция выполняет обработку пакета и возрващает время необходимое на его обработку
        /// </summary>
        /// <param name="proccessingTime">Время выполнения задания с текущем типом</param>
        /// <returns>Время выполнения всех заданий в пакете</returns>
        public int GetTime(int proccessingTime)
        {
            return Size * proccessingTime;
        }
    }
}
