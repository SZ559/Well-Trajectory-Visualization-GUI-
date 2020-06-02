using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace ValueObject
{
    [BsonIgnoreExtraElements]
    [BsonDiscriminator("trajectory")]
    public class Trajectory
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        [BsonElement(elementName: "_id")]
        public ObjectId MongoDbId { get; set; }

        [BsonElement("nodes")]
        public List<PointIn3D> Nodes { get; set; }

        [BsonElement("sourceFilePath")]
        public string SourceFilePath { get; set; }

        [BsonElement("wellName")]
        public string WellName { get; set; }

        [BsonElement("trajectoryName")]
        public string TrajectoryName { get; set; }

        [BsonElement("unit")]
        [BsonRepresentation(BsonType.String)]
        public DistanceUnit Unit { get; set; }

        //[BsonConstructor]
        public Trajectory(string soureFilePath, string wellName, string trajectoryName, DistanceUnit unit = DistanceUnit.Meter)
        {
            SourceFilePath = soureFilePath;
            WellName = wellName;
            TrajectoryName = trajectoryName;
            Unit = unit;
            Nodes = new List<PointIn3D>();
        }

        public PointIn3D this[int index]
        {
            get
            {
                if (index < Nodes.Count)
                {
                    return Nodes[index];
                }
                else
                {
                    throw new ArgumentOutOfRangeException("index");
                }
            }
        }

        public void AddNode(Vector3 node)
        {
            var p = new PointIn3D(node.X, node.Y, node.Z);
            Nodes.Add(p);
        }

        public void AddNode(float x, float y, float z)
        {
            PointIn3D node = new PointIn3D(x, y, z);
            Nodes.Add(node);
        }
    }
}
