using ICities;

namespace SaveOurSaves
{
    public class Mod : IUserMod
    {
        public string Name
        {
            get { return "Save Our Saves";}
        }

        public string Description
        {
            get { return "Fixes save games that would otherwise remain broken"; }
        }
    }
}
