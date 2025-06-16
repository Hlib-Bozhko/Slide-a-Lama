
using System;

namespace Datas.Entity
{
    [Serializable]
    public class Rating
    {
        public int id { get; set; }
        public string Name { get; set; }
        public int mark { get; set; }
    }
}
