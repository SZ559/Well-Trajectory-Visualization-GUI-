using System;
using Xunit;
using GeometricObject;
using System.Numerics;
using System.Collections.Generic;

namespace WellTest
{
    public class SourcesProperty
    {
        private Well well = new Well("Well1");

        [Fact]
        public void GetSources_ReturnSources()
        {
            var expectedSource = new List<string>();
            var actualSource = well.Sources;
            Assert.Equal(expectedSource, actualSource);
        }

        [Fact]
        public void InputSources_SetSources()
        {
            var expectedSource = new List<string>();
            expectedSource.Add("Source1");
            well.Sources = expectedSource;
            var actualSource = well.Sources;
            Assert.Equal(expectedSource, actualSource);
        }
    }

    public class TrajectoriesProperty
    {
        private Well well = new Well("Well1");

        [Fact]
        public void GetTrajectories_ReturnTrajectories()
        {
            var expectedTrajectory = new List<Trajectory>();
            var actualTrajectory = well.Trajectories;
            Assert.Equal(expectedTrajectory, actualTrajectory);
        }

        [Fact]
        public void InputTrajectories_SetTrajectories()
        {
            var expectedTrajectories = new List<Trajectory>();
            Trajectory trajectory1 = new Trajectory("source", "wellName", "trajectoryName");
            expectedTrajectories.Add(trajectory1);
            well.Trajectories = expectedTrajectories;
            var actualTrajectory = well.Trajectories;
            Assert.Equal(expectedTrajectories, actualTrajectory);
        }
    }

    public class TrajectoryCountProperty
    {
        private Well well = new Well("Well1");

        [Fact]
        public void GetTrajectoryCount_ReturnTrajectoryCount()
        {
            var expectedTrajectoryCount = 0;
            var actualTrajectoryCount = well.TrajectoryCount;
            Assert.Equal(expectedTrajectoryCount, actualTrajectoryCount);
        }

        [Fact]
        public void InputTrajectoryCount_SetTrajectoryCount()
        {
            var expectedTrajectoryCount = 5;
            well.TrajectoryCount = expectedTrajectoryCount;
            var actualTrajectoryCount = well.TrajectoryCount;
            Assert.Equal(expectedTrajectoryCount, actualTrajectoryCount);
        }
    }

    public class WellNameProperty
    {
        private Well well = new Well("Well1");

        [Fact]
        public void GetWellName_ReturnWellName()
        {
            var expectedWellName = "Well1";
            var actualWellName = well.WellName;
            Assert.Equal(expectedWellName, actualWellName);
        }

        [Fact]
        public void InputWellName_SetWellName()
        {
            var expectedWellName = "ExpectedWellName";
            well.WellName = expectedWellName;
            var actualWellName = well.WellName;
            Assert.Equal(expectedWellName, actualWellName);
        }
    }

    public class WellConstructor
    {
        [Fact]
        public void InputWellName_SetWellName()
        {
            var expectedWellName = "Well1";
            Well well = new Well(expectedWellName);
            var actualWellName = well.WellName;
            Assert.Equal(expectedWellName, actualWellName);
        }

        [Fact]
        public void InitializeSources()
        {
            Well well = new Well("Well1");
            var expected = new List<string>();
            var actual = well.Sources;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InitializeTrajectories()
        {
            Well well = new Well("Well1");
            var expected = new List<Trajectory>();
            var actual = well.Trajectories;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void InitializeTrajectoryCount()
        {
            Well well = new Well("Well1");
            var expected = 0;
            var actual = well.TrajectoryCount;
            Assert.Equal(expected, actual);
        }
    }

    public class AddTrajectory
    {
        private Well well = new Well("Well1");
        [Fact]
        private void InputTrajectory_AddTrajectory()
        {
            var expectedTrajectories = new List<Trajectory>();
            Trajectory trajectory1 = new Trajectory("source", "wellName", "trajectoryName");
            trajectory1.polyLineNodes.Add(new Vector3(1, 1, 1));
            expectedTrajectories.Add(trajectory1);
            well.AddTrajectory(trajectory1);
            var actualTrajectories = well.Trajectories;

            Assert.Equal(expectedTrajectories, actualTrajectories);
        }
    }
}