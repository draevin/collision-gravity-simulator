using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GameLogic;
using System.Timers;
using System.ComponentModel;

namespace CollisionDetection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<BallV> balls = new List<BallV>();
        public List<Body> bodies = new List<Body>();


        public bool creatorIsBody = false;
        public Vec2 creatorVelocity = new Vec2(0,0);
        public int creatorRadius = 20;


        // main window with constant canvas size update for edge collisions
        public MainWindow()
        {
            InitializeComponent();
            DrawObjects();
            this.SizeChanged += MainWindow_SizeChanged;
        }

        // actually updating bounds for collisions
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GameLogic.BallV.rightBound = (int)canvasArea.ActualWidth;
            GameLogic.BallV.botBound = (int)canvasArea.ActualHeight;
        }

        //create on click
        private void canvasArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            Vec2 currentVel = new Vec2(0, 0);
            currentVel = currentVel.Add(creatorVelocity);

            var currentRad = creatorRadius;

            if (creatorIsBody)
            {
                Body newBody;
                newBody = new Body((int)e.GetPosition(canvasArea).X, (int)e.GetPosition(canvasArea).Y, 0, 0, currentRad);
                bodies.Add(newBody);
                DrawObjects();

            } else
            {
                BallV newBall;
                newBall = new BallV((int)e.GetPosition(canvasArea).X, (int)e.GetPosition(canvasArea).Y, currentVel, currentRad);
                balls.Add(newBall);
                DrawObjects();
            }
                      
        }

        private void slideChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            float radians = -(float)angSlide.Value * (float)Math.PI / 180;

            creatorRadius = (int)radSlide.Value;
            creatorVelocity = new Vec2(-(float)Math.Sin(radians), -(float)Math.Cos(radians));
            creatorVelocity = creatorVelocity.Multiply((float)velSlide.Value);

        }

        private void radSlideChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            creatorRadius = (int)radSlide.Value;
        }

        private void checkToggle(object sender, RoutedEventArgs e)
        {
            creatorIsBody = (bool)(sender as CheckBox).IsChecked;
        }

        //delete on right click
        private void canvasArea_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition((Canvas)sender);
            HitTestResult result = VisualTreeHelper.HitTest(canvasArea, pt);

            //if a ball is hit
            if (result != null)
            {

                //remove ball from screen
                canvasArea.Children.Remove(result.VisualHit as Shape);

                List<BallV> removeBalls = new List<BallV>();
                List<Body> removeBodies = new List<Body>();

                //check for the underlying Ball (can select duplicates if overlapping and moving with the same speed/direction)
                foreach (BallV ball in balls)
                {
                    if (ball == null || (result.VisualHit as Shape) == null) break;
                    if (ball.position.x - ball.radius == Canvas.GetLeft(result.VisualHit as Shape) && ball.position.y - ball.radius == Canvas.GetTop(result.VisualHit as Shape))
                    {
                        removeBalls.Add(ball);
                    }
                }

                foreach (Body body in bodies)
                {
                    if (body == null || (result.VisualHit as Shape) == null) break;
                    if (body.position.x - body.radius == Canvas.GetLeft(result.VisualHit as Shape) && body.position.y - body.radius == Canvas.GetTop(result.VisualHit as Shape))
                    {
                        removeBodies.Add(body);
                    }
                }

                //remove underlying ball
                balls = balls.Except(removeBalls).ToList();
                bodies = bodies.Except(removeBodies).ToList();

            }
        }

        //prevent closing during timer, causing null exception
        private void Window_closing(object sender, CancelEventArgs e)
        {
            if (t != null)
            {
                t.Stop();
                t = null;
            }
        }


        private Timer t;
        private TimeSpan elapsed;

        //start button
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (t != null)
            {
                t.Stop();
                t = null;
            }
            else
            {
                t = new Timer(50);
                elapsed = new TimeSpan(0);
                t.Elapsed += (sender2, e2) =>
                {                    
                    Application.Current.Dispatcher.Invoke(new Action(() => fullStep()));
                };
                t.AutoReset = false;
                t.Start();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {

            List<BallV> removeBalls = new List<BallV>();

            foreach (BallV ball in balls)
            {
                removeBalls.Add(ball);               
            }

            balls = balls.Except(removeBalls).ToList();
            canvasArea.Children.Clear();
        }


        //step all balls and continue frames
        private void fullStep()
        {
            canvasArea.Children.Clear();
            if (balls != null)
            {
                List<BallV> checkedBalls = new List<BallV>();

                foreach (BallV ball1 in balls)
                {
                    //GRAVITY:
                    //ball1.velocity.y += 1;
                    ball1.Step();

                    checkedBalls.Add(ball1);

                    foreach (BallV ball2 in balls.Except(checkedBalls))
                    {
                        if (ball1.BallsColliding(ball2))
                            ball1.courseCollision(ball2);
                    }                

                    foreach (Body body in bodies)
                    {
                        if (ball1.UnderEffect(body))
                            ball1.gravitate(body);
                    }
                }
                
                Application.Current.Dispatcher.Invoke(new System.Action(() => DrawObjects()));
            }

            if (t != null) t.Start();
        }

        //drawing each ball based on position and size
        private void DrawObjects()
        {
            if (balls != null)
            {
                foreach (BallV ball in balls)
                {
                    Shape Rendershape = new Ellipse() { Height = ball.radius * 2, Width = ball.radius * 2 };
                    RadialGradientBrush brush = new RadialGradientBrush();
                    brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#D6D6D6"), 0.250));
                    brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#D6D6D6"), 0.100));
                    brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#D6D6D6"), 8));
                    Rendershape.Fill = brush;

                    Canvas.SetLeft(Rendershape, ball.position.x-ball.radius);
                    Canvas.SetTop(Rendershape, ball.position.y-ball.radius);

                    canvasArea.Children.Add(Rendershape);
                }
            }

            if (bodies != null)
            {
                foreach (Body body in bodies)
                {
                    Shape Rendershape = new Ellipse() { Height = body.radius * 2, Width = body.radius * 2 };
                    RadialGradientBrush brush = new RadialGradientBrush();
                    brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF4D4D"), 0.250));
                    brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF4D4D"), 0.100));
                    brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF4D4D"), 8));
                    Rendershape.Fill = brush;

                    Canvas.SetLeft(Rendershape, body.position.x - body.radius);
                    Canvas.SetTop(Rendershape, body.position.y - body.radius);

                    canvasArea.Children.Add(Rendershape);
                }
            }
        }
    }
}
