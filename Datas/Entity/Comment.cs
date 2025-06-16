using System;

namespace Datas.Entity
{
    [Serializable]
    public class Comment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
    }
}
