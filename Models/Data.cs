using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triangulation.Models
{
    internal class Data
    {
        public static List<Router> Routers = new List<Router>();

        public static Priemnik Priemnik = new Priemnik();

        public static void CalculateDistance() // Расстояние от роутера до приемника ((x2 - x1)² + (y2 - y1)²)
        {
            foreach (var router in Routers)
            {
                router.Distance = Math.Sqrt(
                    Math.Pow((router.Coordinates.X - Priemnik.Coordinates.X), 2) + 
                    Math.Pow((router.Coordinates.Y - Priemnik.Coordinates.Y),2)
                    );
            }
        }
    }
}