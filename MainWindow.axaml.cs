using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System.ComponentModel;
using Triangulation.Models;
using System.Collections.Generic;
using Avalonia.Interactivity;
using System.Linq;
using System;
using Avalonia.Input;

namespace Triangulation
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Point _kursorPosition;
        private Ellipse _kursorEllipse;

        private double _xCoord;
        public double XCoord
        {
            get { return _xCoord; }
            set
            {
                if (_xCoord != value)
                {
                    _xCoord = value;
                    OnPropertyChanged(nameof(XCoord));
                }
            }
        }

        private double _yCoord;
        public double YCoord
        {
            get { return _yCoord; }
            set
            {
                if (_yCoord != value)
                {
                    _yCoord = value;
                    OnPropertyChanged(nameof(YCoord));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            draw_krug();
            OtrezokEllipses();
            DataContext = this;
        }

        private void draw_krug()
        {
            for (int i = 0; i < 3; i++)
            {
                var krug = new Ellipse
                {
                    Width = 300,
                    Height = 300,
                    Opacity = 50,
                    Fill = Brushes.Blue,
                    ZIndex = 2
                };

                Canvas.SetLeft(krug, i * 250);
                Canvas.SetTop(krug, 25);

                krug.Tag = i + 1;

                pole.Children.Add(krug);

                var newRouter = new Router
                {
                    Id = i + 1,
                    Coordinates = new Point(
                        Canvas.GetLeft(krug) + krug.Width / 2,
                        Canvas.GetTop(krug) + krug.Height / 2
                    ),
                    Frequency = 2.4,
                    Radius = krug.Width / 2,
                };

                Data.Routers.Add(newRouter);
            }

            var redKrug = new Ellipse
            {
                Width = 30,
                Height = 30,
                Fill = Brushes.Green,
                ZIndex = 3
            };

            Canvas.SetLeft(redKrug, 50);
            Canvas.SetTop(redKrug, 50);

            pole.Children.Add(redKrug);

            pole.PointerPressed += Pole_PointerPressed;
            pole.PointerMoved += Pole_PointerMoved;
            pole.PointerReleased += Pole_PointerReleased;
            pole.DoubleTapped += Pole_DoubleTapped;
        }

        private void Pole_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            _kursorEllipse = null;
            _kursorPosition = e.GetPosition((Canvas)sender);

            foreach (var krug in pole.Children)
            {
                if (krug is Ellipse kruzhok && kruzhok.IsPointerOver)
                {
                    _kursorEllipse = kruzhok;
                    break;
                }
            }
        }

        private void Pole_PointerMoved(object sender, Avalonia.Input.PointerEventArgs e)
        {
            if (_kursorEllipse != null)
            {
                var currentkursorposition = e.GetPosition((Canvas)sender);

                var raznica = currentkursorposition - _kursorPosition;

                Canvas.SetLeft(_kursorEllipse, Canvas.GetLeft(_kursorEllipse) + raznica.X);
                Canvas.SetTop(_kursorEllipse, Canvas.GetTop(_kursorEllipse) + raznica.Y);

                _kursorPosition = currentkursorposition;

                UpdateKrugCoordinatesAfterMove();
                OtrezokEllipses();
            }
        }

        private void Pole_PointerReleased(object sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            _kursorEllipse = null;
        }

        private void UpdateKrugCoordinatesAfterMove()
        {
            if (_kursorEllipse != null && (_kursorEllipse.Fill == Brushes.Aquamarine || _kursorEllipse.Fill == Brushes.Blue))
            {
                var routerId = (int)_kursorEllipse.Tag;
                var router = Data.Routers.FirstOrDefault(r => r.Id == routerId);
                if (router != null)
                {
                    router.Coordinates = new Point(
                        Canvas.GetLeft(_kursorEllipse) + _kursorEllipse.Width / 2,
                        Canvas.GetTop(_kursorEllipse) + _kursorEllipse.Height / 2
                    );
                }
            }
        }

        private List<Router> FindNearestRouters()
        {
            var nearestRouters = Data.Routers.OrderBy(d => d.Distance).Take(3).ToList();

            var allEllipses = pole.Children.OfType<Ellipse>().Where(e => e.Fill == Brushes.Blue || e.Fill == Brushes.Aquamarine).ToList();

            foreach (var ellipse in allEllipses)
            {
                ellipse.Fill = Brushes.Aquamarine;
            }

            foreach (var router in nearestRouters)
            {
                var ellipse = allEllipses.FirstOrDefault(e => (int)e.Tag == router.Id);
                if (ellipse != null)
                {
                    ellipse.Fill = Brushes.Blue;
                }
            }

            return nearestRouters;
        }

        private void DeleteLines()
        {
            var lines = pole.Children.OfType<Line>().ToList();
            foreach (var line in lines)
            {
                pole.Children.Remove(line);
            }
        }

        // Метод для проверки нахождения точки внутри всех кругов роутеров
        private bool IsPointInsideCircles(Point point, List<Router> routers)
        {
            foreach (var router in routers)
            {
                double distance =
                    Math.Sqrt(Math.Pow(point.X - router.Coordinates.X, 2) +
                              Math.Pow(point.Y - router.Coordinates.Y, 2));
                if (distance > router.Radius)
                {
                    return false; // Если точка находится вне одного из кругов
                }
            }
            return true; // Если точка находится внутри всех кругов
        }

        // Метод для проверки нахождения точки хотя бы в одном круге
        private bool IsPointInsideAnyCircle(Point point, List<Router> routers)
        {
            foreach (var router in routers)
            {
                double distance = Math.Sqrt(Math.Pow(point.X - router.Coordinates.X, 2) + Math.Pow(point.Y - router.Coordinates.Y, 2));
                if (distance <= router.Radius)
                {
                    return true; // Если точка находится хотя бы в одном круге
                }
            }
            return false; // Если точка не находится ни в одном круге
        }

        private void OtrezokEllipses()
        {
            Data.Priemnik = new Priemnik();
            var bluePrimenik = (Ellipse)pole.Children.FirstOrDefault(c => c is Ellipse && ((Ellipse)c).Fill == Brushes.Green);
            if (bluePrimenik != null)
            {
                Data.Priemnik.Coordinates =
                    new Point(Canvas.GetLeft(bluePrimenik) + bluePrimenik.Width / 2,
                               Canvas.GetTop(bluePrimenik) + bluePrimenik.Height / 2);
            }
            Data.CalculateDistance();

            var nearestRouters = FindNearestRouters();

            DeleteLines();

            for (int i = 0; i < nearestRouters.Count; i++)
            {
                for (int j = i + 1; j < nearestRouters.Count; j++)
                {
                    var line =
                        new Line { StrokeThickness = 1, Stroke = Brushes.Black, ZIndex = 4, StartPoint = nearestRouters[i].Coordinates, EndPoint = nearestRouters[j].Coordinates };
                    pole.Children.Add(line);
                }
            }

            foreach (var router in nearestRouters)
            {
                var line =
                    new Line { StrokeThickness = 1, Stroke = Brushes.Black, ZIndex = 4, StartPoint = router.Coordinates, EndPoint = Data.Priemnik.Coordinates };
                pole.Children.Add(line);
            }

            bool isInsideAllCircles = IsPointInsideCircles(Data.Priemnik.Coordinates, nearestRouters);

            // Вычисляем координаты, только если приемник в пересечении всех кругов
            if (isInsideAllCircles)
            {
                Reshenie(nearestRouters);
                OnPropertyChanged(nameof(XCoord));
                OnPropertyChanged(nameof(YCoord));
            }
            else
            {
                // Обнуляем координаты, если приемник не в пересечении всех кругов
                XCoord = 0;
                YCoord = 0;
                OnPropertyChanged(nameof(XCoord));
                OnPropertyChanged(nameof(YCoord));
            }

            // Рассчитываем и отображаем силу сигнала, если приемник находится в зоне покрытия хотя бы одного роутера
            if (IsPointInsideAnyCircle(Data.Priemnik.Coordinates, Data.Routers))
            {
                ShowSignalKachestvo(nearestRouters); // Отображаем силу сигнала для ближайших роутеров
            }
            else
            {
                SignalTextBlock.Text = "Приемник вне зоны покрытия любого роутера";
            }
        }

        private void Reshenie(List<Router> routers)
        {
            foreach (var router in routers)
            {
                if (router.Distance > router.Radius)
                {
                    return;
                }
            }

            double[,] matrica = new double[2, 3];
            matrica[0, 0] = 2 * (routers[0].Coordinates.X - routers[1].Coordinates.X);
            matrica[0, 1] = 2 * (routers[0].Coordinates.Y - routers[1].Coordinates.Y);
            matrica[0, 2] = Math.Pow(routers[0].Coordinates.X, 2) - Math.Pow(routers[1].Coordinates.X, 2) +
                            Math.Pow(routers[0].Coordinates.Y, 2) - Math.Pow(routers[1].Coordinates.Y, 2) -
                            Math.Pow(routers[0].Distance, 2) + Math.Pow(routers[1].Distance, 2);

            matrica[1, 0] = 2 * (routers[0].Coordinates.X - routers[2].Coordinates.X);
            matrica[1, 1] = 2 * (routers[0].Coordinates.Y - routers[2].Coordinates.Y);
            matrica[1, 2] = Math.Pow(routers[0].Coordinates.X, 2) - Math.Pow(routers[2].Coordinates.X, 2) +
                            Math.Pow(routers[0].Coordinates.Y, 2) - Math.Pow(routers[2].Coordinates.Y, 2) -
                            Math.Pow(routers[0].Distance, 2) + Math.Pow(routers[2].Distance, 2);

            double determinant = (matrica[0, 0] * matrica[1, 1]) -
                                (matrica[0, 1] * matrica[1, 0]);

            if (determinant != 0)
            {
                double x = (matrica[0, 2] * matrica[1, 1] - matrica[1, 2] * matrica[0, 1]) / determinant;
                double y = (matrica[0, 0] * matrica[1, 2] - matrica[1, 0] * matrica[0, 2]) / determinant;

                XCoord = x;
                YCoord = y;

                OnPropertyChanged(nameof(XCoord));
                OnPropertyChanged(nameof(YCoord));
            }
        }

        private void ShowSignalKachestvo(List<Router> nearestRouters)
        {
            if (nearestRouters.Count >= 3)
            {
                SignalTextBlock.Text = $"Первый: {nearestRouters[0].SignalKachestvo}%\nВторой: {nearestRouters[1].SignalKachestvo}%\nТретий: {nearestRouters[2].SignalKachestvo}%";
            }
            else
            {
                SignalTextBlock.Text = "Недостаточно данных для отображения силы сигнала";
            }
        }

        private void Pole_DoubleTapped(object sender, TappedEventArgs e)
        {
            if (_kursorEllipse != null && (_kursorEllipse.Fill == Brushes.Aquamarine || _kursorEllipse.Fill == Brushes.Blue))
            {
                var krugTag = (int?)_kursorEllipse.Tag ?? 0;
                var krug_in_spisok = Data.Routers.FirstOrDefault(k => k.Id == krugTag);
                if (krug_in_spisok != null)
                {
                    if (Data.Routers.Count <= 3)
                    {
                        return;
                    }
                    else
                    {
                        Data.Routers.Remove(krug_in_spisok);
                        pole.Children.Remove(_kursorEllipse);

                        OtrezokEllipses();
                    }
                }
            }
        }

        private void ExitButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddRouter_Test_ButtonClick(object sender, RoutedEventArgs e)
        {
            var ellipse = new Ellipse
            {
                Width = 300,
                Height = 300,
                Fill = Brushes.Aquamarine,
                Opacity = 50,
                ZIndex = 2,
                Tag = Data.Routers.Max(r => r.Id) + 1
            };
            Canvas.SetLeft(ellipse, 2 * 150 + 0);
            Canvas.SetTop(ellipse, 20);
            var newRouter = new Router
            {
                Id = (int)ellipse.Tag,
                Coordinates = new Point(
                       Canvas.GetLeft(ellipse) + ellipse.Width / 2,
                       Canvas.GetTop(ellipse) + ellipse.Height / 2
                   ),
                Frequency = 2.4,
                Radius = ellipse.Width / 2
            };
            Data.Routers.Add(newRouter);

            pole.Children.Add(ellipse);

            FindNearestRouters();
            OtrezokEllipses();
        }
    }
}