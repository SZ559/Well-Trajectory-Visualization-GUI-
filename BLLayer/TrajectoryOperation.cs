using System.Collections.Generic;
using DataSource;
using ValueObject;
using MongoDB.Bson;
using System.Linq;
using MongoDB.Bson.Serialization;

namespace BLLayer
{
    public class TrajectoryOperation
    {
        private TrajectoryDatabase database;
        private TrajectoryDataFileReader fileReader;

        public TrajectoryOperation()
        {
            database = new TrajectoryDatabase();
            fileReader = new TrajectoryDataFileReader();
        }

        public TrajectoryOperation(TrajectoryDatabase trajectoryDatabase)
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

        public List<Trajectory> GetAllTrajectoriesOfOneWell(string wellName)
        {
            return GetAllTrajectories().FindAll(x => x.WellName == wellName).ToList();
        }

        public List<string> GetAllWells()
        {
            return GetAllTrajectories().Select(x => x.WellName).Distinct().ToList();
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
                if (!ids.Contains(trajectory.MongoDbId))
                {
                    if(wellNames.Contains(trajectory.WellName))
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

            foreach(var id in ids)
            {
                searchResult.Add(BsonSerializer.Deserialize<Trajectory>(database.GetTrajectoryInBsonDocumentByTrajectoryObjectID(id)));
            }

            return searchResult;
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
