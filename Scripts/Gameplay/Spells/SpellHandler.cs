namespace Gameplay.Player.Spells
{
    public class SpellHandler
    {
        private readonly SpellData data;

        public SpellType SpellType => data.SpellType;

        public SpellHandler(SpellData data)
        {
            this.data = data;
        }
    }
}