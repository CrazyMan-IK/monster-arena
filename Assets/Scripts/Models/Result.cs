namespace MonsterArena.Models
{
    public class Result
    {
        public int Kills { get; set; }
        public float DieTime { get; set; }

        public Result() : this(0, float.MaxValue)
        {

        }

        public Result(int kills, float livedTime)
        {
            Kills = kills;
            DieTime = livedTime;
        }
    }
}
