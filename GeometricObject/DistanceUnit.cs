using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeometricObject
{
    [Flags]
    public enum DistanceUnit
    {
        Meter,
        Feet,
    }

    public class UnitConversion
    {
        public static float Result(DistanceUnit unit, DistanceUnit unitInUse)
        {
            switch (unit)
            {
                case DistanceUnit.Meter:
                    switch (unitInUse)
                    {
                        case DistanceUnit.Feet:
                            return 3.2808399F;
                        case DistanceUnit.Meter:
                        default:
                            return 1;
                    }
                case DistanceUnit.Feet:
                default:
                    switch (unitInUse)
                    {
                        case DistanceUnit.Meter:
                            return 0.3048F;
                        case DistanceUnit.Feet:
                        default:
                            return 1;
                    }
            }

        }
    }
}
