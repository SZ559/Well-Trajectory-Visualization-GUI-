using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeometricObject
{
    [Flags]
    public enum DistanceUnit
    {
        Meter,
        Feet,
    }

    public class UnitConvertor
    {
        public static float[] MeterToFeet(params float[] numbers)
        {
            return numbers.Select(x => (float)(x * 3.2808399)).ToArray();
        }

        public static float[] FeetToMeter(params float[] numbers)
        {
            return numbers.Select(x => (float)(x * 0.3048)).ToArray();
        }

        public static float MeterToFeet(float number)
        {
            return (float)(number * 3.2808399);
        }

        public static float FeetToMeter(float number)
        {
            return (float)(number * 0.3048);
        }
    }
}
