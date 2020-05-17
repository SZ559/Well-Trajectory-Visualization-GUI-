using System;
using Xunit;
using GeometricObject;
using System.Numerics;
using System.Collections.Generic;

namespace PointIn2DTest
{
    public class XProperty
    {
        private PointIn2D pointIn2D = new PointIn2D(1, 2, "s");

        [Fact]
        public void GetX_ReturnX()
        {
            var actual = pointIn2D.X;
            float expected = 1;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InputX_SetX()
        {
            float expected = 5;
            pointIn2D.X = expected;
            var actual = pointIn2D.X;
            Assert.Equal(expected, actual);
        }
    }

    public class YProperty
    {
        private PointIn2D pointIn2D = new PointIn2D(1, 2, "s");

        [Fact]
        public void GetY_ReturnY()
        {
            var actual = pointIn2D.Y;
            float expected = 2;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InputY_SetY()
        {
            float expected = 5;
            pointIn2D.Y = expected;
            var actual = pointIn2D.Y;
            Assert.Equal(expected, actual);
        }
    }

    public class AnnotationProperty
    {
        private PointIn2D pointIn2D = new PointIn2D(1, 2, "s");

        [Fact]
        public void GetY_ReturnY()
        {
            var actual = pointIn2D.Annotation;
            var expected = "s";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InputY_SetY()
        {
            var expected = "x";
            pointIn2D.Annotation = expected;
            var actual = pointIn2D.Annotation;
            Assert.Equal(expected, actual);
        }
    }

    public class ToString
    {
        private PointIn2D pointIn2D = new PointIn2D(1, 2, "s");

        [Fact]
        public void ReturnString()
        {
            var actual = pointIn2D.ToString();
            var expected = "(" + 1 + "," + 2 + ")";
            Assert.Equal(expected, actual);
        }
    }

    public class IsEqual
    {
        private PointIn2D pointIn2D = new PointIn2D(1, 2, "s");

        [Fact]
        public void InputPointIn2DWithSameXAndSameY_ReturnTrue()
        {
            PointIn2D pointIn2D2 = new PointIn2D(1, 2, "S");
            var actual = pointIn2D.IsEqual(pointIn2D2);
            Assert.True(actual);
        }

        [Theory]
        [InlineData(2, 2)]
        [InlineData(1, 3)]
        public void InputPointIn2DWithDifferentXOrY_ReturnFalse(float x, float y)
        {
            PointIn2D pointIn2D2 = new PointIn2D(x, y, "S");
            var actual = pointIn2D.IsEqual(pointIn2D2);
            Assert.False(actual);
        }
    }


}
