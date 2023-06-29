using System;

namespace NS_Education.Models.Entities
{
    public partial class B_SiteData
    {
        public int Size => Math.Max(MaxSize ?? 0, BasicSize);
    }
}