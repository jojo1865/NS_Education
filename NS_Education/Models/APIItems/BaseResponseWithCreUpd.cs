namespace NS_Education.Models.APIItems
{
    public abstract class BaseResponseWithCreUpd<TEntity>
      where TEntity : class
    {
        public bool ActiveFlag { get; set; }
        
        public string CreDate { get; set; }
        public string CreUser { get; set; }
        public int CreUID { get; set; }
        public string UpdDate { get; set; }
        public string UpdUser { get; set; }
        public int UpdUID { get; set; }
    }
}