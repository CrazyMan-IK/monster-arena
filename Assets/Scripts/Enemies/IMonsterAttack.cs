namespace MonsterArena
{
    public interface IMonsterAttack
    {
        void StartAttack();
        void Hit(float damage);
        void Reset();
    }
}