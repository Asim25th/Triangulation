using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triangulation.Models
{
    internal class Router
    {
        public int Id { get; set; }
        public Point Coordinates { get; set; } // Координаты точки Wi-Fi
        public double Frequency { get; set; } // Частота
        public double Radius { get; set; } // Радиус зоны покрытия
        public double Distance { get; set; } // Расстояние до приемника (рассчитывать по формуле)
        public int SignalKachestvo
        {
            get
            {
                // Вычисляем коэффициент как отношение Distance к Radius
                double ratio = Distance / Radius;

                // Приводим значение в диапазон от 0 до 1 и умножаем на 100
                int result = (int)((1 - ratio) * 100);

                // Убедимся, что результат не меньше 0
                return Math.Max(result, 0);
            }
        }
    }
}