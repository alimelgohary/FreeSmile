namespace FreeSmile.Models
{
    public partial class CaseType
    {
        public string Lang(string language)
        {
            if (language.ToLower() == "ar")
            {
                return NameAr;
            }
            else
            {
                return NameEn;
            }
        }
    }
}
