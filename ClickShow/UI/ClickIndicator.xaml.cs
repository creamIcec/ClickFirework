using ClickShow.Settings;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ClickShow
{
    /// <summary>
    /// Interaction logic for ClickIndicator.xaml
    /// </summary>
    public partial class ClickIndicator : Window
    {
        private Storyboard _mouseDownStoryBoard;
        private Storyboard _mouseUpStoryBoard;
        private double halfWidth;
        private double halfHeight;
        public int LastLiveTime { get; set; } = Environment.TickCount;
        const double interval = 0.5;
        private byte maxRed;
        private byte maxGreen;
        private byte maxBlue;
        private int particleSize;

        public ClickIndicator(double size, byte maxRed, byte maxGreen, byte maxBlue, int particleSize)
        {
            ShowActivated = false;
            InitializeComponent();

            this.Width = size;
            this.Height = size;

            this.halfWidth = Width / 2;
            this.halfHeight = Height / 2;

            SourceInitialized += OnSourceInitialized;
            DpiChanged += OnDpiChanged;

            RenderOptions.SetBitmapScalingMode(TheCircle, BitmapScalingMode.LowQuality);

            this.maxRed = maxRed;
            this.maxGreen = maxGreen;
            this.maxBlue = maxBlue;
            this.particleSize = particleSize;

            CreateMouseDownStoryBoard();
            CreateMouseUpStoryBoard();
            
            //Play();
        }

        /// <summary>
        /// 鼠标抬起特效
        /// </summary>
        private void CreateMouseUpStoryBoard()
        {
            // 初始化鼠标按下动画
            _mouseUpStoryBoard = new Storyboard();
            _mouseUpStoryBoard.FillBehavior = FillBehavior.Stop;

            generateParticles(_mouseUpStoryBoard);

            _mouseUpStoryBoard.Completed += MouseDownStoryBoardOnCompleted;
            if (_mouseUpStoryBoard.CanFreeze)
            {
                _mouseUpStoryBoard.Freeze();
            }
        }

        /// <summary>
        /// 鼠标按下特效
        /// </summary>
        private void CreateMouseDownStoryBoard()
        {
            // 初始化动画鼠标抬起动画
            
            _mouseDownStoryBoard = new Storyboard();
            _mouseDownStoryBoard.FillBehavior = FillBehavior.Stop;

            generateParticles(_mouseDownStoryBoard);

            _mouseDownStoryBoard.Completed += MouseDownStoryBoardOnCompleted;
            if (_mouseDownStoryBoard.CanFreeze)
            {
                _mouseDownStoryBoard.Freeze();
            }
        }

        private void generateParticles(Storyboard board)
        {
            Random random = new Random();
            EllipseGeometry[] els = new EllipseGeometry[15];
            System.Windows.Shapes.Path[] paths = new System.Windows.Shapes.Path[15];
            for (int i = 0; i < els.Length; i++)
            {
                Object objEl = this.FindName("c" + i);
                Object objPath = this.FindName("p" + i);
                try
                {
                    els[i] = (EllipseGeometry)objEl;
                    paths[i] = (System.Windows.Shapes.Path)objPath;
                }
                catch
                {
                    throw new Exception("没有找到对应元素。看看是不是忘记添加了?");
                }
            }
            for (int i = 0; i < els.Length; i++)
            {
                // Set the From and To properties of the animation.
                PointAnimation moveAnimation = new PointAnimation
                {
                    From = new Point(this.Width / 2, this.Height / 2),
                    To = new Point(random.Next(0, (int)this.Width + 1), random.Next(0, (int)(this.Height + 1))),
                    Duration = TimeSpan.FromSeconds(interval)
                };
                byte r = (byte)random.Next(0, maxRed);
                byte g = (byte)random.Next(0, maxGreen);
                byte b = (byte)random.Next(0, maxBlue);
                var color = Color.FromArgb(255, r, g, b);
                // 创建一个Brush对象
                Brush brush = new SolidColorBrush(color);
                paths[i].Stroke = brush;
                els[i].RadiusX = particleSize;
                els[i].RadiusY = particleSize;
                // 将Brush对象赋值给Path的Stroke属性
                Storyboard.SetTarget(moveAnimation, els[i]);
                Storyboard.SetTargetProperty(
                    moveAnimation, new PropertyPath(EllipseGeometry.CenterProperty));
                board.Children.Add(moveAnimation);
            }
        }

        private void OnDpiChanged(object sender, DpiChangedEventArgs e)
        {
            DpiHasChanged = true;
            _currentDpi = e.NewDpi;
        }

        public double GetDpiScale()
        {
            if (_currentDpi.DpiScaleX < 0.1)
            {
                _currentDpi = VisualTreeHelper.GetDpi(this);


            }

            return _currentDpi.DpiScaleX;
        }


        public bool IsIdle { get; private set; } = false;

        private DpiScale _currentDpi;



        public bool DpiHasChanged { get; private set; } = false;

        public void Prepare()
        {
            IsIdle = false;
        }

        public void Play(Brush circleBrush, bool isDown)
        {
            this.LastLiveTime = Environment.TickCount;
            Opacity = isDown ? 0.95 : 0.7;

            // 抬起特效


            var color = Color.FromArgb(0, 255, 255, 255);
            Brush brush = new SolidColorBrush(color);
            TheCircle.Stroke = brush;

            IsIdle = false;

            if (isDown)
            {
                TheCircle.Width = this.Width * 0.2;
                TheCircle.Height = this.Height * 0.2;
                _mouseDownStoryBoard.Begin();
            }
            else
            {
                _mouseUpStoryBoard.Begin();
            }


            this.Show();
        }



        private void OnSourceInitialized(object sender, EventArgs e)
        {
            WindowHelper.SetWindowExTransparent(new WindowInteropHelper(this).Handle);
        }

        private void MouseDownStoryBoardOnCompleted(object sender, EventArgs e)
        {

            TheCircle.Width = 0;
            TheCircle.Height = 0;
            this.Opacity = 0;

            IsIdle = true;
            DpiHasChanged = false;
        }
    }
}
