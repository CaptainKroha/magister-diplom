namespace newAlgorithm.Model.TimeChangeoverMatrix
{
    public class TreeDimMatrixNode
    {
        public int DeviceNumber { get; } = 0;
        /// <summary>
        /// Тип заданий
        /// </summary>
        public int Type { get; } = 0;
        /// <summary>
        /// Позиция в матрице
        /// </summary>
        public int Position { get; } = 0;
        /// <summary>
        /// Количество пакетов в текущей позиции
        /// </summary>
        public int Count { get; } = 0;

        public TreeDimMatrixNode(int deviceNumber, int type, int position, int count)
        {
            DeviceNumber = deviceNumber;
            Type = type;
            Position = position;
            Count = count;
        }
    }
}
