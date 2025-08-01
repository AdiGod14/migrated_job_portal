using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using System;
using MigratedJobPortalAPI.Models;


namespace MigratedJobPortalAPI
{
    public class ApplicationStatusStringSerializer : IBsonSerializer<ApplicationStatus>
    {
        public Type ValueType => typeof(ApplicationStatus);

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ApplicationStatus value)
        {
            var str = value switch
            {
                ApplicationStatus.InProgress => "In Progress",
                _ => value.ToString()
            };

            context.Writer.WriteString(str);
        }

        public ApplicationStatus Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var str = context.Reader.ReadString();
            return str switch
            {
                "In Progress" => ApplicationStatus.InProgress,
                "Applied" => ApplicationStatus.Applied,
                "Accepted" => ApplicationStatus.Accepted,
                "Rejected" => ApplicationStatus.Rejected,
                _ => throw new ArgumentException($"Unknown application status: {str}")
            };
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
            => Serialize(context, args, (ApplicationStatus)value);

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            => Deserialize(context, args);
    }
}