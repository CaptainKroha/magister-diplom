using System.Collections.Generic;

namespace magisterDiplom.Model
{
    public class MatrixY
    {
        protected readonly int _rows;
        protected int _columns = 0;
        protected readonly List<List<int>> _data;

        public MatrixY(int rows)
        {
            _rows = rows;
            _data = new List<List<int>>(rows);
            for(int i = 0; i < _rows; i++)
            {
                _data.Add(new List<int>());
            }
        }

        public int Columns { get { return _columns; } }

        public void AddColumn()
        {
            for(int i = 0; i < _rows; ++i)
            {
                _data[i].Add(0);
            }
            _columns++;
        }

        public void SetPreMaintenceLastPacketAllDevices()
        {
            for(int i = 0; i < _rows; ++i)
            {
                SetPreMaintence(i, _columns - 1);
            }  
        }

        public virtual void SetPreMaintence(int device, int packet)
        {
            _data[device][packet] = 1;
        }

        public virtual void UnsetPreMaintence(int device, int packet)
        {
            _data[device][packet] = 0;
        }

        public int PreMaintenceStatusAfter(int device, int packet)
        {
            return _data[device][packet];
        }

        public void Clear()
        {
            for (int i = 0; i < _rows; ++i)
                for (int j = 0; j < _columns; ++j)
                    _data[i][j] = 0;

        }

        public static List<List<int>> ToListList(MatrixY matrix)
        {
            return matrix._data;
        }

    }

    public class MatrixYPreMTypes : MatrixY
    {
        public MatrixYPreMTypes(int rows) : base(rows)
        { }

        public int PreMaintenceStatusAfter(int packet)
        {
            int result = 0;
            for (int i = 0; i < _rows; i++)
            {
                if (_data[i][packet] == 1)
                {
                    result = 1;
                    break;
                }
            }
            return result;
        }


        public void SetPreMaintenceLastPacketFirstType()
        {
            SetPreMaintence(0, _columns - 1);
        }

        public override void SetPreMaintence(int preMType, int packet)
        {
            _data[preMType][packet] = 1;
        }

        public override void UnsetPreMaintence(int preMType, int packet)
        {
            _data[preMType][packet] = 0;
        }
    }
}
