using System;
using Zenject;

namespace PlayVibe
{
    public class ScreenFaderFactory : PlaceholderFactory<ScreenFaderBase>
    {
        public ScreenFaderBase GetFader(ScreenFaderProfile profile)
        {
            return DetermineFadeType(profile);
        }

        protected ScreenFaderBase DetermineFadeType(ScreenFaderProfile profile)
        {
            switch (profile.ScreenFaderType)
            {
                case ScreenFaderType.Fade:
                    return new ScreenFaderFadeIn(profile);
                case ScreenFaderType.Scale:
                    return new ScreenFaderScaleIn(profile);
                case ScreenFaderType.Move:
                    return new ScreenFaderMoveIn(profile);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}