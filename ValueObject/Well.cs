using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValueObject
{
    [BsonIgnoreExtraElements]
    public class Well
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        [BsonElement(elementName: "_id")]
        public ObjectId MongoDbId { get; set; }

        [BsonElement("sources")]
        public List<string> Sources { get; set; }

        [BsonElement("trajectories")]
        public List<Trajectory> Trajectories { get; set; }

        [BsonElement("wellName")]
        public string WellName { get; set; }

        //[BsonConstructor]
        public Well(string wellName)
        {
            Sources = new List<string>();
            Trajectories = new List<Trajectory>();
            WellName = wellName;
        }

        public void AddTrajectory(Trajectory trajectory)
        {
            Trajectories.Add(trajectory);
            Sources.Add(trajectory.SourceFilePath);
        }
    }
}
