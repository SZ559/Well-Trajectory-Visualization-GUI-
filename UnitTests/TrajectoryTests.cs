using System;
using Xunit;
using GeometricObject;
using System.Numerics;
using System.Collections.Generic;

namespace TrajectoryTest
{
    public class PolyLineNodesProperty
    {
        [Fact]
        public void GetPolyLineNodes_ReturnPolyLineNodes()
        {
            Trajectory trajectory = new Trajectory("source", "Well1", "Trajectory1");
            var expected = new List<Vector3>();
            var expectectedPoint = new Vector3(1, 1, 1);
            expected.Add(expectectedPoint);
            trajectory.PolyLineNodes.Add(expectectedPoint);
            var actual = trajectory.PolyLineNodes;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InputPolyLineNodes_SetPolyLineNodes()
        {
            Trajectory trajectory = new Trajectory("source", "Well1", "Trajectory1");
            var expected = new List<Vector3>();
            var expectectedPolyLineNode = new Vector3(1, 1, 1);
            expected.Add(expectectedPolyLineNode);
            trajectory.PolyLineNodes = expected;
            var actual = trajectory.PolyLineNodes;
            Assert.Equal(expected, actual);
        }
    }

    public class SourceFileProperty
    {
        [Fact]
        public void GetSourceFile_ReturnSourceFile()
        {
            var expectedSourceFile = "source";
            Trajectory trajectory = new Trajectory(expectedSourceFile, "Well1", "Trajectory1");
            var actualSourceFile = trajectory.SourceFile;
            Assert.Equal(expectedSourceFile, actualSourceFile);
        }

        [Fact]
        public void InputSourceFile_SetSourceFile()
        {
            Trajectory trajectory = new Trajectory("s", "Well1", "Trajectory1");
            var expectedSourceFile = "source";
            trajectory.SourceFile = expectedSourceFile;
            var actualSourceFile = trajectory.SourceFile;
            Assert.Equal(expectedSourceFile, actualSourceFile);
        }
    }

    public class TrajectoryNameProperty
    {
        [Fact]
        public void GetTrajectoryName_ReturnTrajectoryName()
        {
            var expectedTrajectoryName = "ExpectedTrajectory";
            Trajectory trajectory = new Trajectory("source", "Well1", expectedTrajectoryName);
            var actualTrajectoryName = trajectory.TrajectoryName;
            Assert.Equal(expectedTrajectoryName, actualTrajectoryName);
        }

        [Fact]
        public void InputTrajectoryName_SetTrajectoryName()
        {
            Trajectory trajectory = new Trajectory("source", "Well1", "Trajectory1");
            var expectedTrajectoryName = "ExpectedTrajectory";
            trajectory.TrajectoryName = expectedTrajectoryName;
            var actualTrajectoryName = trajectory.TrajectoryName;
            Assert.Equal(expectedTrajectoryName, actualTrajectoryName);
        }
    }

    public class WellNameProperty
    {
        [Fact]
        public void GetWellName_ReturnWellName()
        {
            var expectedWellName = "ExpectedWell";
            Trajectory trajectory = new Trajectory("source", expectedWellName, "Trajectory1");
            var actualWellName = trajectory.WellName;
            Assert.Equal(expectedWellName, actualWellName);
        }

        [Fact]
        public void InputWellName_SetWellName()
        {
            Trajectory trajectory = new Trajectory("source", "Well1", "Trajectory1");
            var expectedWellName = "ExpectedWell";
            trajectory.WellName = expectedWellName;
            var actualWellName = trajectory.WellName;
            Assert.Equal(expectedWellName, actualWellName);
        }
    }

    public class TrajectoryConstructor
    {
        private Trajectory trajectory = new Trajectory("source", "Well1", "Trajectory1");

        [Fact]
        public void InputSourceFile_SetSourceFile()
        {
            var expectedSourceFile = "source";
            var actualSourceFile = trajectory.SourceFile;
            Assert.Equal(expectedSourceFile, actualSourceFile);
        }

        [Fact]
        public void InputWellName_SetWellName()
        {
            var expectedWellName = "Well1";
            var actualWellName = trajectory.WellName;
            Assert.Equal(expectedWellName, actualWellName);
        }

        [Fact]
        public void InputTrajectoryName_SetTrajectoryName()
        {
            var expectedTrajectoryName = "Trajectory1";
            var actualTrajectoryName = trajectory.TrajectoryName;
            Assert.Equal(expectedTrajectoryName, actualTrajectoryName);
        }

        [Fact]
        public void InitializeEmptyPolyLineNodes()
        {
            var expected = new List<Vector3>();
            var actual = trajectory.PolyLineNodes;
            Assert.Equal(expected, actual);
        }
    }

    public class AddNode
    {
        private Trajectory trajectory = new Trajectory("source", "Well1", "Trajectory1");
        [Fact]
        public void InputPolyLineNode_AddPolyLineNode()
        {
            var expectedPolyLineNodes = new List<Vector3>();
            var expectedPolyLineNode = new Vector3(1, 1, 1);
            expectedPolyLineNodes.Add(expectedPolyLineNode);
            trajectory.AddNode(expectedPolyLineNode);
            var actualPolyLineNodes = trajectory.PolyLineNodes;

            Assert.Equal(expectedPolyLineNodes, actualPolyLineNodes);
        }

        [Fact]
        public void InputXYZCoordinateOfPolyLineNode_AddPolyLineNode()
        {
            var expectedPolyLineNodes = new List<Vector3>();
            var expectedPolyLineNode = new Vector3(1, 1, 1);
            expectedPolyLineNodes.Add(expectedPolyLineNode);
            trajectory.AddNode(expectedPolyLineNode.X, expectedPolyLineNode.Y, expectedPolyLineNode.Z);
            var actualPolyLineNodes = trajectory.PolyLineNodes;
            Assert.Equal(expectedPolyLineNodes, actualPolyLineNodes);
        }
    }
}