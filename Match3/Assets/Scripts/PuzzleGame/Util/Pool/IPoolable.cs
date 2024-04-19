namespace PuzzleGame.Util.Pool
{
    public interface IPoolable
    {
        /// <summary>
        ///     Called before returning the instance back to Pool,
        ///     to normalize and get rid of residues from previous usage.
        /// </summary>
        void OnDespawn();

        void OnSpawn();
    }
}