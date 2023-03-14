namespace TouchDetector
{
    public class TouchPosition
    {
        private static TouchPosition _singleton;

        public delegate void ClickedDone(int X, int Y);

        public double X { get; set; }
        public double Y { get; set; }

        public int MaxX { get; set; }

        public int MaxY { get; set; }

        private readonly int screenHeight;

        private readonly int screenWidth;

        public string ButtonPressed { get; set; }

        public static TouchPosition GetTouch()
        {
            if (_singleton == null)
                _singleton = new TouchPosition();
            return _singleton;
        }

        public TouchPosition()
        {
            screenWidth = Screen.PrimaryScreen.Bounds.Size.Width;
            screenHeight = Screen.PrimaryScreen.Bounds.Size.Height;
        }

        public static void SetTouchPosition(int x, int y, int maxX, int maxY)
        {
            if (_singleton == null)
                _singleton = new TouchPosition();
            _singleton.X = x;
            _singleton.Y = y;
            _singleton.MaxX = maxX;
            _singleton.MaxY = maxY;
            _singleton.ConvertCoordinates();
        }

        private void ConvertCoordinates()
        {
            _singleton.X = Convert.ToInt32(_singleton.X / Convert.ToDouble(MaxX / screenWidth));
            _singleton.Y = Convert.ToInt32(_singleton.Y / Convert.ToDouble(MaxY / screenHeight));
        }

    }
}
