﻿using GeoLocation.Model;
using GeoLocation.Repository.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using GeoLocation.Model.Common;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Data.Common;

namespace GeoLocation.Repository
{
    public class EventRepository : IEventRepository
    {
        private NpgsqlConnection conn = null;
        private IConfiguration _configuration;
        private string _conStr = string.Empty;

        public EventRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _conStr = _configuration.GetConnectionString("MainConnection");
        }

        public async Task<IEnumerable<Event>> GetEvents()
        {
            List<Event> events = new List<Event>();
            using (conn = new NpgsqlConnection(_conStr))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM \"Event\"" +
                    "INNER JOIN \"EventCategory\" ON \"Event\".\"EventCategoryId\" = \"EventCategory\".\"Id\"" +
                    "INNER JOIN \"EventSubCategory\" ON \"Event\".\"EventSubCategoryId\" = \"EventSubCategory\".\"Id\"" +
                    "INNER JOIN \"Venue\" ON \"Event\".\"VenueId\" = \"Venue\".\"Id\"" +
                    "INNER JOIN \"Status\" ON \"Event\".\"StatusId\" = \"Status\".\"Id\"", conn);
                DbDataReader dr = await command.ExecuteReaderAsync();
                while (dr.Read())
                {
                    Event newEvent = new Event()
                    {
                        Id = (Guid)dr["Id"],
                        Name = (string)dr["Name"],
                        Description = (string)dr["Description"],
                        EntryFee = (Decimal)dr["EntryFee"],
                        LimitedSpace = (int)dr["LimitedSpace"],
                        Organizer = (string)dr["Organizer"],
                        Lat = (double)dr["Lat"],
                        Long = (double)dr["Long"],
                        StartDate = (DateTime)dr["StartDate"],
                        EndDate = (DateTime)dr["EndDate"],
                        EventCategoryId = (dr["EventCategoryId"] is DBNull) ? Guid.Empty : (Guid)dr["EventCategoryId"],
                        EventSubCategoryId = (dr["EventSubCategoryId"] is DBNull) ? Guid.Empty : (Guid)dr["EventSubcategoryId"],
                        VenueId = (dr["VenueId"] is DBNull) ? Guid.Empty : (Guid)dr["VenueId"],
                        StatusId = (dr["StatusId"] is DBNull) ? Guid.Empty : (Guid)dr["StatusId"],
                        // joined columns
                        CategoryName = (string)dr["CategoryName"],
                        SubCategoryName = (string)dr["SubCategoryName"],
                        VenueName = (string)dr["VenueName"],
                        StatusAbrv = (string)dr["StatusAbrv"]
                    };
                    events.Add(newEvent);
                }
            }
            return events;
        }


        public void AddEvent(IEvent newEvent)
        {
            using (conn = new NpgsqlConnection(_conStr))
            {
                conn.Open();
                using (var command = new NpgsqlCommand())
                {
                    command.Connection = conn;
                    command.CommandText = "INSERT INTO \"Event\" (\"Id\", \"Name\", \"Description\"" +
                        ", \"EntryFee\", \"LimitedSpace\", \"Organizer\", \"Lat\", \"Long\", \"StartDate\", \"EndDate\", \"EventCategoryId\", \"EventSubCategoryId\", \"VenueId\", \"StatusId\")" +
                        "VALUES (@id, @name, @desc" +
                        ", @fee, @lspace, @org, @lat, @long, @start, @end, @catId, @subCatId, @venueId, @statusId)";
                    command.Parameters.AddWithValue("id", newEvent.Id);
                    command.Parameters.AddWithValue("name", newEvent.Name);
                    command.Parameters.AddWithValue("desc", newEvent.Description);
                    command.Parameters.AddWithValue("fee", newEvent.EntryFee);
                    command.Parameters.AddWithValue("lspace", newEvent.LimitedSpace);
                    command.Parameters.AddWithValue("org", newEvent.Organizer);
                    command.Parameters.AddWithValue("lat", newEvent.Lat);
                    command.Parameters.AddWithValue("long", newEvent.Long);
                    command.Parameters.AddWithValue("start", newEvent.StartDate);
                    command.Parameters.AddWithValue("end", newEvent.EndDate);
                    command.Parameters.AddWithValue("catId", newEvent.EventCategoryId);
                    command.Parameters.AddWithValue("subCatId", newEvent.EventSubCategoryId);
                    command.Parameters.AddWithValue("venueId", newEvent.VenueId);
                    command.Parameters.AddWithValue("statusId", newEvent.StatusId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteEvent(Guid eventId)
        {
            using (conn = new NpgsqlConnection(_conStr))
            {
                conn.Open();
                using (var command = new NpgsqlCommand())
                {
                    command.Connection = conn;
                    command.CommandText = "DELETE FROM \"Comment\" WHERE \"EventId\" = @id; " +
                        "DELETE FROM \"Rsvp\" WHERE \"EventId\" = @id; " +
                        "DELETE FROM \"Image\" WHERE \"EventId\" = @id; " +
                        "DELETE FROM \"Rating\" WHERE \"EventId\" = @id; " + 
                        "DELETE FROM \"Event\" WHERE \"Id\" = @id;";
                    command.Parameters.AddWithValue("id", eventId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Event GetEventById(Guid eventId)
        {
            using (conn = new NpgsqlConnection(_conStr))
            {
                conn.Open();
                using (var command = new NpgsqlCommand())
                {
                    command.Connection = conn;
                    command.CommandText = "SELECT * FROM \"Event\" WHERE \"Id\" = @id";
                    command.Parameters.AddWithValue("id", eventId);
                    var dr = command.ExecuteReader();
                    dr.Read();
                    Event newEvent = new Event()
                    {
                        Id = (Guid)dr["Id"],
                        Name = (string)dr["Name"],
                        Description = (string)dr["Description"],
                        EntryFee = (Decimal)dr["EntryFee"],
                        LimitedSpace = (int)dr["LimitedSpace"],
                        Organizer = (string)dr["Organizer"],
                        Lat = (double)dr["Lat"],
                        Long = (double)dr["Long"],
                        StartDate = (DateTime)dr["StartDate"],
                        EndDate = (DateTime)dr["EndDate"],
                        EventCategoryId = (dr["EventCategoryId"] is DBNull) ? Guid.Empty : (Guid)dr["EventCategoryId"],
                        EventSubCategoryId = (dr["EventSubCategoryId"] is DBNull) ? Guid.Empty : (Guid)dr["EventSubcategoryId"],
                        VenueId = (dr["VenueId"] is DBNull) ? Guid.Empty : (Guid)dr["VenueId"],
                        StatusId = (dr["StatusId"] is DBNull) ? Guid.Empty : (Guid)dr["StatusId"]
                    };
                    return newEvent;
                }
            }
        }

        public void UpdateEvent(Event updatedEvent)
        {
            using (conn = new NpgsqlConnection(_conStr))
            {
                conn.Open();
                using (var command = new NpgsqlCommand())
                {
                    command.Connection = conn;
                    command.CommandText = "UPDATE \"Event\"" +
                        "SET \"Name\" = @name, " +
                        "\"Description\" = @desc, " +
                        "\"EntryFee\" = @entryFee, " +
                        "\"LimitedSpace\" = @limitedSpace, " +
                        "\"Organizer\" = @org, " +
                        "\"Lat\" = @lat, " +
                        "\"Long\" = @long, " +
                        "\"StartDate\" = @startDate, " +
                        "\"EndDate\" = @endDate, " +
                        "\"EventCategoryId\" = @catId, " +
                        "\"EventSubCategoryId\" = @subCatId, " +
                        "\"VenueId\" = @venueId, " +
                        "\"StatusId\" = @statusId " +
                        "WHERE \"Id\" = @id;";
                    command.Parameters.AddWithValue("name", updatedEvent.Name);
                    command.Parameters.AddWithValue("desc", updatedEvent.Description);
                    command.Parameters.AddWithValue("entryFee", updatedEvent.EntryFee);
                    command.Parameters.AddWithValue("limitedSpace", updatedEvent.LimitedSpace);
                    command.Parameters.AddWithValue("org", updatedEvent.Organizer);
                    command.Parameters.AddWithValue("lat", updatedEvent.Lat);
                    command.Parameters.AddWithValue("long", updatedEvent.Long);
                    command.Parameters.AddWithValue("startDate", updatedEvent.StartDate);
                    command.Parameters.AddWithValue("endDate", updatedEvent.EndDate);
                    command.Parameters.AddWithValue("catId", updatedEvent.EventCategoryId);
                    command.Parameters.AddWithValue("subCatId", updatedEvent.EventSubCategoryId);
                    command.Parameters.AddWithValue("venueId", updatedEvent.VenueId);
                    command.Parameters.AddWithValue("statusId", updatedEvent.StatusId);
                    command.Parameters.AddWithValue("id", updatedEvent.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Status CheckStatus(Event newEvent)
        {
            string oldStatusAbrv = newEvent.StatusAbrv;
            string newStatusAbrv;
            Status newStatus = new Status();

            if (DateTime.Now.CompareTo(newEvent.StartDate) > 0 && DateTime.Now.CompareTo(newEvent.EndDate) < 0)
            {
                newStatusAbrv = "Now";
            }

            else if (DateTime.Now.CompareTo(newEvent.EndDate) > 0)
            {
                newStatusAbrv = "Ended";
            }

            else
            {
                newStatusAbrv = "Upcoming";
            }

            using (conn = new NpgsqlConnection(_conStr))
            {
                conn.Open();
                using (var command = new NpgsqlCommand())
                {
                    command.CommandText = "SELECT * FROM \"Status\" WHERE \"StatusAbrv\" = @newAbrv";
                    command.Parameters.AddWithValue("newAbrv", newStatusAbrv);
                    command.Connection = conn;
                    using (var dr = command.ExecuteReader())
                    {
                        dr.Read();
                        newStatus.Id = (Guid)dr["Id"];
                        newStatus.Abrv = (string)dr["StatusAbrv"];
                        newStatus.Name = (string)dr["StatusName"];
                    }
                }
            }

            if (oldStatusAbrv != newStatusAbrv)
            {
                using (conn = new NpgsqlConnection(_conStr))
                {
                    conn.Open();
                    using (var command = new NpgsqlCommand())
                    {
                        command.CommandText = "UPDATE \"Event\" " +
                            "SET \"StatusId\" = @newStatusId " +
                            "WHERE \"Id\" = @eventId";
                        command.Parameters.AddWithValue("newStatusId", newStatus.Id);
                        command.Parameters.AddWithValue("eventId", newEvent.Id);
                        command.Connection = conn;
                        command.ExecuteNonQuery();
                    }
                }
            }

            return newStatus;
        }
    }
}
