namespace newAlgorithm.Model
{

    /// <summary>
    /// Данный класс описывает узел трёхмерной матрицы представляющий из себя матрицу переналадки приборов с одного типа задания на другой
    /// </summary>
    public class TreeDimMatrixNode
    {

        /// <summary>
        /// Данная переменная описывает устройство на котором происходит переналадка
        /// </summary>
        public int device { get; }

        /// <summary>
        /// Данная переменная описывает тип задания с которого выполняется переналадка
        /// </summary>
        public int fromDataType { get; }

        /// <summary>
        /// Данная переменная описывает тип задания на которое выполняется переналадка
        /// </summary>
        public int toDataType { get; }

        /// <summary>
        /// Данная переменная описывает временные затраты на переналадку прибора device с типа fromDataType на тип toDataType
        /// </summary>
        public int time { get; private set; }

        /// <summary>
        /// Данный конструктор создаём экземпляр узла трёхмерной матрицы
        /// </summary>
        /// <param name="device">Прибор на котором выполняется переналадка</param>
        /// <param name="fromDataType">Тип данных с которого выполняется перенладка</param>
        /// <param name="toDataType">Тип данных на который выполняется перенладка</param>
        /// <param name="time">Время переналадки</param>
        public TreeDimMatrixNode(int device, int fromDataType, int toDataType, int time)
        {
            this.device = device;
            this.fromDataType = fromDataType;
            this.toDataType = toDataType;
            this.time = time;
        }

        public void SetTime(int value)
        {
            this.time = value;
        }
    }
}
