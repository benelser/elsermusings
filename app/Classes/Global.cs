namespace app 
{
    public class Globals
    {
        public string OrgId { get; }
        public string SiteName { get; }


        public Globals()
        {
            OrgId = "53555500-6c99-4478-b945-566328b3b343";
            SiteName = "Elser's Musings";
        }

        public static Globals GetGlobals()
        {
            return new Globals();
        }

    }
}

