namespace FargowiltasSouls.Core.Toggler
{
    public class Toggle
    {
        public string Mod;
        public string Catagory;
        public string InternalName;
        public bool ToggleBool;

        public Toggle(string internalName, string mod, string catagory)
        {
            InternalName = internalName;
            Mod = mod;
            Catagory = catagory;

            ToggleBool = true;
        }

        public override string ToString() => $"Mod: {Mod}, Catagory: {Catagory}, InternalName: {InternalName}, Toggled: {ToggleBool}";
    }
}
