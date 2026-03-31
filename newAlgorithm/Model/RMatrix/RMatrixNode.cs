namespace newAlgorithm.Model
{
    
    /// <summary>
    /// Определяет множество узлов матрицы R и определяется как количетсво данных типа Type в партии Position
    /// TODO: Представление узла матрицы R происходит по переменной позиции, было бы лучше переписать матрицу на словарь
    /// </summary>
    public class RMatrixNode
    {

        /// <summary>
        /// Данная переменная представляет из себя тип задания данного узла
        /// </summary>
        public int dataType { get; }

        /// <summary>
        /// Данная переменная представляет из себя позицию в матрице данного узла
        /// </summary>
        public int position { get; }

        /// <summary>
        ///  Данная переменная содержит в себе количество пакетов в текущей позиции текущего типа данного узла
        /// </summary>
        public int batchCount { get; private set; }

        /// <summary>
        /// Данный конструктор принимает 3 параметра и возвращает экземпляр класса узла матрицы R
        /// </summary>
        /// <param name="dataType">Определяет тип задания, данного узла</param>
        /// <param name="position">Определяет позицию задания, данного узла</param>
        /// <param name="batchCount">Определяет количество данных в данном узле</param>
        public RMatrixNode(int dataType, int position, int batchCount)
        {
            this.dataType = dataType;
            this.position = position;
            this.batchCount = batchCount;
        }

        /// <summary>
        /// Данная функция выполняет установку данных количества пакетов
        /// </summary>
        /// <param name="batchCount">Количество пакетов</param>
        public void setBatchCount(int batchCount)
        {
            this.batchCount = batchCount;
        }
    }
}
