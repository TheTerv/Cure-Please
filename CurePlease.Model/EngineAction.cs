
namespace CurePlease.Model
{
    public class EngineAction
    {
        public int Priority { get; set; }
        public string Spell { get; set; }
        public string Target { get; set; }
        public string JobAbility { get; set; }
        public string Message { 
            get {
                if (!string.IsNullOrEmpty(JobAbility))
                {
                    if(!string.IsNullOrEmpty(Spell))
                    {
                        return $"Using {JobAbility} + Casting {Spell} on {Target}";
                    }

                    return $"Using {JobAbility} on {Target}";
                }

                return $"Casting {Spell} on {Target}";
            } 
        }
    }
}
