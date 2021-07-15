namespace ErogeHelper.Common.Entity
{
    public enum TouchButtonCorner
    {
        Left,
        Top,
        Right,
        Bottom,
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight
    }

    public class AssistiveTouchPosition
    {
        public AssistiveTouchPosition(TouchButtonCorner corner, double scale = 0)
        {
            Corner = corner;
            Scale = scale;
        }

        public TouchButtonCorner Corner { get; }
        public double Scale { get; }
    }
}
