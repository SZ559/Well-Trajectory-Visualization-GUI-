using System;
using Xunit;
using GeometricObject;
using System.Numerics;
using System.Collections.Generic;

namespace ProjectionTest
{
    public class GetProjectionView
    {
        private Projection projection = new Projection();
        [Fact]
        public void InputNormalVectorParallelToXAxis_ReturnLeft()
        {
            var expected = "Left";
            var actual = projection.GetProjectionView(new Vector3(3, 0, 0));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InputNormalVectorParallelToYAxis_ReturnMain()
        {
            var expected = "Main";
            var actual = projection.GetProjectionView(new Vector3(0, 4, 0));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InputNormalVectorParallelToZAxis_ReturnTop()
        {
            var expected = "Top";
            var actual = projection.GetProjectionView(new Vector3(0, 0, 1));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InputNormalVectorNotParallelToXAixsAndYAxisAndZAxis_ReturnProjection()
        {
            var expected = "Projection";
            var actual = projection.GetProjectionView(new Vector3(0, 1, 1));
            Assert.Equal(expected, actual);
        }
    }

    public class GetCoordinatesOfPointIn3D_AfterProjection_UsingNormalVector
    {
        private Projection projection = new Projection();

        [Fact]
        public void InputNormalVectorPointToProjectAndOnePointOnProjectionPlane_ReturnCoordinatesOfPointIn3DAfterProjection()
        {
            var normalVector = new Vector3(1, 1, 0);
            var pointToProject = new Vector3(1, 1, 1);
            var onePointOnProjectionPlane = new Vector3(0, 0, 0);
            var expected = new Vector3(0, 0, 1);
            var actual = projection.GetCoordinatesOfPointIn3D_AfterProjection_UsingNormalVector(normalVector, pointToProject, onePointOnProjectionPlane);
            Assert.Equal(expected, actual);
        }
    }

    public class GetCoordinatesOfProjection_InPlaneThroughOrigin
    {
        private Projection projection = new Projection();
        [Fact]
        public void InputPointProjectionAndUnitXProjection_ReturnCoordinateOfProjection_InPositiveYDirection()
        {
            var pointProjection = new Vector3(0, 0, 1);
            var unitXProjection = new Vector3((float)0.5, (float)-0.5, (float)0);
            var actual = projection.GetCoordinatesOfProjection_InPlaneThroughOrigin(pointProjection, unitXProjection);
            var expected = new PointIn2D((float)0, (float)1);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InputPointProjectionAndUnitXProjection_ReturnCoordinateOfProjection_InNegativeYDirection()
        {
            var pointProjection = new Vector3(0, 0, -1);
            var unitXProjection = new Vector3((float)0.5, (float)-0.5, (float)0);
            var actual = projection.GetCoordinatesOfProjection_InPlaneThroughOrigin(pointProjection, unitXProjection);
            var expected = new PointIn2D((float)0, (float)-1);
            Assert.Equal(expected, actual);
        }
    }

    public class GetProjectionInPlane
    {
        private Projection projection = new Projection();

        [Fact]
        public void InputWellTrajectoryAndNormalVectorParallelToXAxis_ReturnProjectionInPlane()
        {
            var wellTrajectory = new List<Vector3>();
            wellTrajectory.Add(new Vector3(1, 2, 3));
            var normalVector = new Vector3(3, 0, 0);

            var actual = projection.GetProjectionInPlane(wellTrajectory, normalVector);
            var expected = new List<PointIn2D>();
            expected.Add(new PointIn2D(2, 3));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InputWellTrajectoryAndNormalVectorParallelToYAxis_ReturnProjectionInPlane()
        {
            var wellTrajectory = new List<Vector3>();
            wellTrajectory.Add(new Vector3(1, 2, 3));
            var normalVector = new Vector3(0, 1, 0);

            var actual = projection.GetProjectionInPlane(wellTrajectory, normalVector);
            var expected = new List<PointIn2D>();
            expected.Add(new PointIn2D(1, 3));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InputWellTrajectoryAndNormalVectorParallelToZAxis_ReturnProjectionInPlane()
        {
            var wellTrajectory = new List<Vector3>();
            wellTrajectory.Add(new Vector3(1, 2, 3));
            var normalVector = new Vector3(0, 0, 2);

            var actual = projection.GetProjectionInPlane(wellTrajectory, normalVector);
            var expected = new List<PointIn2D>();
            expected.Add(new PointIn2D(1, 2));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InputWellTrajectoryAndNormalVectorNotParallelToXAixsAndYAxisAndZAxis_ReturnProjectionInPlane()
        {
            var wellTrajectory = new List<Vector3>();
            wellTrajectory.Add(new Vector3(1, 1, 1));
            var normalVector = new Vector3(1, 1, 0);

            var actual = projection.GetProjectionInPlane(wellTrajectory, normalVector);
            var expected = new List<PointIn2D>();
            expected.Add(new PointIn2D(0, 1));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InputWellTrajectoryAndNormalVectorNotParallelToXAixsAndYAxisAndZAxis_ReturnProjectionInPlane_Example2()
        {
            var wellTrajectory = new List<Vector3>();
            wellTrajectory.Add(new Vector3(1, 1, -1));
            var normalVector = new Vector3(1, 1, 0);

            var actual = projection.GetProjectionInPlane(wellTrajectory, normalVector);
            var expected = new List<PointIn2D>();
            expected.Add(new PointIn2D(0, -1));
            Assert.Equal(expected, actual);
        }
    }
}