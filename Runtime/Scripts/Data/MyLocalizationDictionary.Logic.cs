namespace MLoc.Data
{
    using System.Linq;

    public partial class MyLocalizationDictionary
    {
        public MyTranslatePair GetTagByGuid(string guid) =>
            string.IsNullOrEmpty(guid) ? null : _translatePairs?.FirstOrDefault(x => x.LocalizedTag == guid);

        public void AddTranslatePair(MyTranslatePair pair)
        {
            if (pair == null)
            {
                return;
            }

            if (_translatePairs == null)
            {
                _translatePairs = new();
            }

            _translatePairs.Add(pair);
        }

        public void SortLocalizationTags()
        {

        }
    }
}
