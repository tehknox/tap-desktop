﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
    //the class for an airliner manufacturer
    [Serializable]
    public class Manufacturer : ISerializable
    {
        [Versioning("name")]
        public string Name { get; set; }
        [Versioning("shortname")]
        public string ShortName { get; set; }
        [Versioning("country")]
        public Country Country { get; set; }
        [Versioning("logo")]
        public string Logo { get; set; }
        [Versioning("isreal")]
        public Boolean IsReal { get; set; }
        public Manufacturer(string name, string shortname, Country country, Boolean isReal)
        {
            this.Name = name;
            this.ShortName = shortname;
            this.Country = country;
            this.IsReal = isReal;

           

        }
           private Manufacturer(SerializationInfo info, StreamingContext ctxt)
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
    //the collection of manufacturers
    public class Manufacturers
    {
        private static List<Manufacturer> manufacturers = new List<Manufacturer>();
        //clears the list
        public static void Clear()
        {
            manufacturers = new List<Manufacturer>();
        }
        //adds a manufacturer to the collection
        public static void AddManufacturer(Manufacturer manufacturer)
        {
            manufacturers.Add(manufacturer);
        }
        //returns a manufacturer
        public static Manufacturer GetManufacturer(string name)
        {
            return manufacturers.Find(m => m.Name == name || m.ShortName == name);
        }
        //returns the list manufacturers
        public static List<Manufacturer> GetAllManufacturers()
        {
            return manufacturers;
        }
        public static List<Manufacturer> GetManufacturers(Predicate<Manufacturer> match)
        {
            return manufacturers.FindAll(match);
        }
    }
}
