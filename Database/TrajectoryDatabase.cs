using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using ValueObject;

namespace DataSource
{
    public class TrajectoryDatabase
    {
        private MongoClient client;
        private string databseName = "WellTrajectory";
        private string trajectoryCollectionName = "trajectory";
        //private string wellCollectionName = "well";
        private IMongoDatabase trajectoryDatabase;
        private IMongoCollection<BsonDocument> trajectoryCollection;
        //private IMongoCollection<BsonDocument> wellCollection;
        public TrajectoryDatabase()
        {
            client = new MongoClient("mongodb://localhost:27017");
            trajectoryDatabase = client.GetDatabase(databseName);
            trajectoryCollection = trajectoryDatabase.GetCollection<BsonDocument>(trajectoryCollectionName);
            //wellCollection = trajectoryDatabase.GetCollection<BsonDocument>(wellCollectionName);
        }

        public List<BsonDocument> GetAllTrajectoriesInBsonDocument()
        {
            var projection = Builders<BsonDocument>.Projection.Exclude("nodes");
            var documents = trajectoryCollection.Find(new BsonDocument()).Project(projection).ToList();
            return documents;
        }

        public BsonDocument GetOneTrajectoryByTrajectoryNameAndWellName(string wellName, string trajectoryName)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("wellName", wellName) & Builders<BsonDocument>.Filter.Eq("trajectoryName", trajectoryName);
            var document = trajectoryCollection.Find(filter).First();
            return document;
        }

        public List<BsonDocument> GetListOfTrajectoriesByTrajectoryNameAndWellName(string wellName, string trajectoryName)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("wellName", wellName) & Builders<BsonDocument>.Filter.Eq("trajectoryName", trajectoryName);
            var document = trajectoryCollection.Find(filter).ToList();
            return document;
        }

        //public BsonDocument GetOneWellByWellName(string wellName)
        //{
        //    var filter = Builders<BsonDocument>.Filter.Eq("wellName", wellName);
        //    return wellCollection.Find(filter).First();
        //}

        public BsonDocument GetTrajectoryInBsonDocumentByTrajectoryObjectID(ObjectId id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var document = trajectoryCollection.Find(filter).First();
            return document;
        }

        public void DeleteOneTrajectoryDocument(ObjectId id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            trajectoryCollection.DeleteOne(filter);
        }

        public void InsertOneTrajectoryDocument(BsonDocument trajectoryInBsonDocument)
        {
            trajectoryCollection.InsertOne(trajectoryInBsonDocument);
        }
    }
}