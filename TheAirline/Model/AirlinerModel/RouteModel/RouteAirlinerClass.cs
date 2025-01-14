﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{
    
    /*! Route airliner class for passengers.
    * This class is used for an airliner class onboard of a route airliner for passengers
    * The class needs parameters for type of class and the fare price
    */
       [Serializable]
     public class RouteAirlinerClass : ISerializable
    {
        // chs, 2011-18-10 added seating type to a route airliner class
        public enum SeatingType { Reserved_Seating, Free_Seating }
        [Versioning("seating")]
        public SeatingType Seating { get; set; }
        [Versioning("fareprice")]
        public double FarePrice { get; set; }
           [Versioning("facilities")]
        public List<RouteFacility> Facilities { get; set; }
        [Versioning("type")]
        public AirlinerClass.ClassType Type { get; set; }
        //public int CabinCrew { get; set; }
        
        public RouteAirlinerClass(AirlinerClass.ClassType type,SeatingType seating, double fareprice)
        {
            this.Facilities = new List<RouteFacility>();
            this.FarePrice =  fareprice;
            this.Seating =  seating;
            this.Type = type;
        }
        //adds a facility to the route class
        public void addFacility(RouteFacility facility)
        {
            if (facility != null)
            {
                if (this.Facilities.Exists(f => f.Type == facility.Type))
                    this.Facilities.RemoveAll(f => f.Type == facility.Type);

                this.Facilities.Add(facility);
            }
        }
        //returns the facility for a type for the route class
        public RouteFacility getFacility(RouteFacility.FacilityType type)
        {
            return this.Facilities.Find(f => f.Type == type);
        }
        //returns all facilities
        public List<RouteFacility> getFacilities()
        {
            return this.Facilities;
        }
   private RouteAirlinerClass(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop = propsAndFields.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);


                if (prop != null)
                {
                    if (prop is FieldInfo)
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    else
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                }
            }

            var notSetProps = propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                Versioning ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    else
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);

                }

            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            var fields = myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                    propValue = ((FieldInfo)member).GetValue(this);
                else
                    propValue = ((PropertyInfo)member).GetValue(this, null);

                Versioning att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }




        }

    }
  
}
