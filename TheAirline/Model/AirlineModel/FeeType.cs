﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlineModel
{
    /*! Fee Type.
     * This class is used for a fee for an airline.
     * The class needs parameters for type, name, defaultvalue, minvalue, maxvalue, percentage
     */
    [Serializable]
    public class FeeType : ISerializable
    {
        public enum eFeeType { Fee, Wage, FoodDrinks, Discount }
        [Versioning("type")]
        public eFeeType Type { get; set; }
        [Versioning("minvalue")]
        private double AMinValue;
        public double MinValue { get { return GeneralHelpers.GetInflationPrice(this.AMinValue); } set { this.AMinValue = value; } }
        [Versioning("maxvalue")]
        private double AMaxValue;
        public double MaxValue { get { return GeneralHelpers.GetInflationPrice(this.AMaxValue); } set { this.AMaxValue = value; } }
        [Versioning("name")]
        public string Name { get; set; }
        [Versioning("default")]
        private double ADefaultValue;
        public double DefaultValue { get { return GeneralHelpers.GetInflationPrice(this.ADefaultValue); } set { this.ADefaultValue = value; } }
        [Versioning("percentage")]
        public int Percentage { get; set; }
        [Versioning("fromyear")]
        public int FromYear { get; set; }
        public FeeType(eFeeType type, string name, double defaultValue, double minValue, double maxValue, int percentage,int fromYear)
        {
            this.Type = type;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.DefaultValue = defaultValue;
            this.Name = name;
            this.Percentage = percentage;
            this.FromYear = fromYear;
        }
        public FeeType(eFeeType type, string name, double defaultValue, double minValue, double maxValue, int percentage)
            : this(type, name, defaultValue, minValue, maxValue, percentage, 1900)
        {
        }
        private FeeType(SerializationInfo info, StreamingContext ctxt)
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
    public class FeeTypes
    {
        private static Dictionary<string, FeeType> types = new Dictionary<string, FeeType>();
        //adds a type to the list
        public static void AddType(FeeType type)
        {
            types.Add(type.Name, type);
        }
        //clears the list
        public static void Clear()
        {
            types = new Dictionary<string, FeeType>();
        }
        //returns the list of fees of a specific type
        public static List<FeeType> GetTypes(FeeType.eFeeType type)
        {
            return GetTypes().FindAll(delegate(FeeType t) { return t.Type == type; });
        }
        //returns the list of fee types
        public static List<FeeType> GetTypes()
        {
            return types.Values.ToList();
        }
        //returns a fee type
        public static FeeType GetType(string name)
        {
            return types[name];
        }

    }
   
}
