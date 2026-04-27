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

        public virtual void SetPreMaintence(int device, int batch)
        {
            _data[device][batch] = 1;
        }

        public virtual void UnsetPreMaintence(int device, int batch)
        {
            _data[device][batch] = 0;
        }

        public int PreMaintenceStatusAfter(int device, int batch)
        {
            return _data[device][batch];
        }

        public void Clear()
        {
            for (int i = 0; i < _rows; ++i)
                for (int j = 0; j < _columns; ++j)
                    _data[i][j] = 0;

        }

        public List<List<int>> ToListList()
        {
            return _data;
        }

    }

    public class MatrixYPreMTypes : MatrixY
    {
        public MatrixYPreMTypes(int rows) : base(rows)
        { }

        public int PreMaintenceStatusAfter(int batch)
        {
            int result = -1;
            for (int i = 0; i < _rows; i++)
            {
                if (_data[i][batch] == 1)
                {
                    result = i;
                    break;
                }
            }
            return result;
        }


        public void SetPreMaintenceLastPacketFirstType()
        {
            SetPreMaintence(0, _columns - 1);
        }

        public override void SetPreMaintence(int preMType, int batch)
        {
            _data[preMType][batch] = 1;
        }

        public override void UnsetPreMaintence(int preMType, int batch)
        {
            _data[preMType][batch] = 0;
        }
    }
}
