using System.Collections.Generic;
using DataSource;
using ValueObject;
using MongoDB.Bson;
using System.Linq;
using MongoDB.Bson.Serialization;
using System;

namespace BLLayer
{
    public class TrajectoryOperator
    {
        private TrajectoryDatabase database;
        private TrajectoryDataFileReader fileReader;

        public TrajectoryOperator()
        {
            database = new TrajectoryDatabase();
            fileReader = new TrajectoryDataFileReader();
        }

        public TrajectoryOperator(TrajectoryDatabase trajectoryDatabase)
        {
            database = trajectoryDatabase;
        }

        public Trajectory GetTrajectoryByTrajectoryNode(ObjectId id)
        {
            var document = database.GetTrajectoryInBsonDocumentByTrajectoryObjectID(id);
            return BsonSerializer.Deserialize<Trajectory>(document);
        }

        public List<Trajectory> GetAllTrajectories()
        {
            return database.GetAllTrajectoriesInBsonDocument().Select(x => BsonSerializer.Deserialize<Trajectory>(x)).ToList();
        }

        public bool HasBeenLoaded(string filePath)
        {
            var allTrajectories = GetAllTrajectories();
            return allTrajectories.Select(x => x.SourceFilePath).Contains(filePath);
        }

        public List<Trajectory> GetTrajectoriesBySearchFunction(string searchText)
        {
            List<Trajectory> searchResult = new List<Trajectory>();
            List<ObjectId> ids = new List<ObjectId>();
            List<string> wellNames = new List<string>();

            foreach (var trajectory in GetAllTrajectories())
            {
                try
                {
                    if (!ids.Contains(trajectory.MongoDbId))
                    {
                        if (wellNames.Contains(trajectory.WellName))
                        {
                            ids.Add(trajectory.MongoDbId);
                        }
                        else if (trajectory.WellName.Contains(searchText))
                        {
                            wellNames.Add(trajectory.WellName);
                            ids.Add(trajectory.MongoDbId);
                        }
                        else if (trajectory.TrajectoryName.Contains(searchText))
                        {
                            ids.Add(trajectory.MongoDbId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO:: how to handle exception - if name is null
                }
            }

            foreach (var id in ids)
            {
                searchResult.Add(BsonSerializer.Deserialize<Trajectory>(database.GetTrajectoryInBsonDocumentByTrajectoryObjectID(id)));
            }

            return searchResult;
        }

        public Dictionary<string, List<Trajectory>> ConstructTreeViewDictionary(List<Trajectory> trajectories)
        {
            Dictionary<string, List<Trajectory>> treeviewDict = new Dictionary<string, List<Trajectory>>();
            foreach (Trajectory trajectory in trajectories)
            {
                try
                {
                    if (!treeviewDict.ContainsKey(trajectory.WellName))
                    {
                        treeviewDict.Add(trajectory.WellName, new List<Trajectory>());
                    }
                    treeviewDict[trajectory.WellName].Add(trajectory);
                }
                catch (Exception ex)
                {
                    //TODO:: how to handle exception - if name is null
                }
            }
            return treeviewDict;
        }

        public void DeleteTrajectoryByTrajectoryNode(ObjectId id)
        {
            database.DeleteOneTrajectoryDocument(id);
        }

        public void DeleteTrajectoriesByWellNode(List<ObjectId> ids)
        {
            foreach (var id in ids)
            {
                database.DeleteOneTrajectoryDocument(id);
            }
        }

        public void LoadTrajectoryToDatabase(Trajectory trajectory)
        {
            database.InsertOneTrajectoryDocument(trajectory.ToBsonDocument());
        }

        public Trajectory ImportTrajectoryFromFile(string filePath, out string errorMessage)
        {
            return fileReader.ReadFile(filePath, out errorMessage);
        }
    }
}
