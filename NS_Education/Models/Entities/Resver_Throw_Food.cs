

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class Resver_Throw_Food
    {
        public int RTFID { get; set; }
        public int RTID { get; set; }
        public int DFCID { get; set; }
        public int BSCID { get; set; }
        public int BPID { get; set; }
        public int Ct { get; set; }
        public int UnitPrice { get; set; }
        public int Price { get; set; }

        public virtual B_Partner BP { get; set; }
        public virtual B_StaticCode BSC { get; set; }
        public virtual D_FoodCategory DFC { get; set; }
        public virtual Resver_Throw RT { get; set; }
    }
}
