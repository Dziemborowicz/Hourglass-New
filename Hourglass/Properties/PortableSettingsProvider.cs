#if PORTABLE

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PortableSettingsProvider.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    using Extensions;

    /// <summary>
    /// A <see cref="SettingsProvider"/> that stores settings in an XML document in the same directory as the assembly.
    /// </summary>
    public sealed class PortableSettingsProvider : SettingsProvider
    {
        /// <summary>
        /// Gets or sets the name of the currently running application.
        /// </summary>
        public override string ApplicationName
        {
            get => Assembly.GetExecutingAssembly().GetName().Name;
#pragma warning disable S3237
            set { /* Do nothing */ }
#pragma warning restore S3237
        }

        /// <summary>
        /// Gets the friendly name used to refer to the provider during configuration.
        /// </summary>
        public override string Name => GetType().FullName!;

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name-value pairs representing the provider-specific attributes
        /// specified in the configuration for this provider.</param>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(ApplicationName, config);
        }

        /// <summary>
        /// Returns the collection of settings property values for the specified application instance and settings
        /// property group.
        /// </summary>
        /// <param name="context">A <see cref="SettingsContext"/> describing the current application use.</param>
        /// <param name="collection">A <see cref="SettingsPropertyCollection"/> containing the settings property group
        /// whose values are to be retrieved.</param>
        /// <returns>A <see cref="SettingsPropertyValueCollection"/> containing the values for the specified settings
        /// property group.</returns>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            SettingsPropertyValueCollection settingsPropertyValueCollection = GetSettingsPropertyValueCollection(collection);

            XmlDocument? document = TryLoadSettingsDocument();
            foreach (SettingsPropertyValue value in settingsPropertyValueCollection)
            {
                string? serializedValue = TryGetSerializedValue(document, value.Name);
                if (!string.IsNullOrWhiteSpace(serializedValue))
                {
                    value.SerializedValue = serializedValue;
                }
            }

            return settingsPropertyValueCollection;
        }

        /// <summary>
        /// Sets the values of the specified group of property settings.
        /// </summary>
        /// <param name="context">A <see cref="SettingsContext"/> describing the current application usage.</param>
        /// <param name="collection">A <see cref="SettingsPropertyValueCollection"/> representing the group of property
        /// settings to set.</param>
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            using XmlWriter writer = GetSettingsDocumentWriter();

            // <?xml version="1.0" encoding="utf-8"?>
            writer.WriteStartDocument();

            // <configuration>
            writer.WriteStartElement("configuration");

            // <userSettings>
            writer.WriteStartElement("userSettings");

            // <{...}.Settings>
            writer.WriteStartElement(typeof(Settings).FullName!);

            foreach (SettingsPropertyValue value in GetSerializableSettingsPropertyValues(collection))
            {
                // <setting name="{...}" serializeAs="{...}">
                writer.WriteStartElement("setting");
                writer.WriteAttributeString("name", value.Name);
                writer.WriteAttributeString("serializeAs", value.Property.SerializeAs.ToString());

                // <value>
                writer.WriteStartElement("value");
                if (value.Property.SerializeAs == SettingsSerializeAs.String)
                {
                    writer.WriteValue(value.SerializedValue.ToString());
                }
                else
                {
                    XmlSerializer serializer = new(value.Property.PropertyType);
                    serializer.Serialize(writer, value.PropertyValue);
                }

                // </value>
                writer.WriteEndElement();

                // </setting>
                writer.WriteEndElement();
            }

            // </{...}.Settings>
            writer.WriteEndElement();

            // </userSettings>
            writer.WriteEndElement();

            // </configuration>
            writer.WriteEndElement();
        }

        /// <summary>
        /// Returns the path to the settings XML document.
        /// </summary>
        /// <returns>The path to the settings XML document.</returns>
        private static string GetSettingsDocumentPath()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileInfo assemblyFileInfo = new(assembly.Location);
            string directory = assemblyFileInfo.DirectoryName ?? ".";
            string fileName = Path.GetFileNameWithoutExtension(assemblyFileInfo.Name) + ".config";
            return Path.Combine(directory, fileName);
        }

        /// <summary>
        /// Returns a <see cref="SettingsPropertyValueCollection"/> containing a <see cref="SettingsPropertyValue"/>
        /// with no value set for each <see cref="SettingsProperty"/> in the <see cref="SettingsPropertyCollection"/>.
        /// </summary>
        /// <param name="collection">A <see cref="SettingsPropertyCollection"/> containing the settings property group
        /// whose values are to be retrieved.</param>
        /// <returns>A <see cref="SettingsPropertyValueCollection"/> containing a <see cref="SettingsPropertyValue"/>
        /// for each <see cref="SettingsProperty"/> in the <see cref="SettingsPropertyCollection"/>.</returns>
        private static SettingsPropertyValueCollection GetSettingsPropertyValueCollection(SettingsPropertyCollection collection)
        {
            SettingsPropertyValueCollection values = [];

            foreach (SettingsProperty property in collection)
            {
                SettingsPropertyValue value = new(property)
                {
                    SerializedValue = property.DefaultValue
                };
                values.Add(value);
            }

            return values;
        }

        /// <summary>
        /// Tries to load the settings XML document.
        /// </summary>
        /// <returns>The settings XML document, or <c>null</c> if the settings XML document could not be loaded.
        /// </returns>
        private static XmlDocument? TryLoadSettingsDocument()
        {
            try
            {
                string path = GetSettingsDocumentPath();
                XmlDocument document = new();
                document.Load(path);
                return document;
            }
            catch (Exception ex) when (ex.CanBeHandled())
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the serialized value of the specified property in an XML document.
        /// </summary>
        /// <param name="document">The XML document.</param>
        /// <param name="propertyName">The name of the property whose value to get.</param>
        /// <returns>The value of the specified property, or <c>null</c> if a value for the specified property was not
        /// found in the XML document.</returns>
        private static string? TryGetSerializedValue(XmlDocument? document, string propertyName)
        {
            if (document is null)
            {
                return null;
            }

            string query = string.Format(CultureInfo.InvariantCulture, "//setting[@name='{0}']/value", propertyName);
            XmlNode? node = document.SelectSingleNode(query);
            return node?.InnerXml;
        }

        /// <summary>
        /// Returns a sorted enumeration of the serializable <see cref="SettingsPropertyValue"/>s.
        /// </summary>
        /// <param name="collection">A <see cref="SettingsPropertyValueCollection"/> representing the group of property
        /// settings to set.</param>
        /// <returns>A sorted enumeration of the serializable <see cref="SettingsPropertyValue"/>s.</returns>
        private static IEnumerable<SettingsPropertyValue> GetSerializableSettingsPropertyValues(SettingsPropertyValueCollection collection)
        {
            return collection
                .Cast<SettingsPropertyValue>()
                .Where(static v => v.Property.SerializeAs is SettingsSerializeAs.String or SettingsSerializeAs.Xml)
                .OrderBy(static v => v.Name, StringComparer.Create(CultureInfo.InvariantCulture, false /* ignoreCase */));
        }

        /// <summary>
        /// Returns a <see cref="XmlWriter"/> for the settings XML document.
        /// </summary>
        /// <returns>A <see cref="XmlWriter"/> for the settings XML document.</returns>
        private static XmlWriter GetSettingsDocumentWriter()
        {
            string path = GetSettingsDocumentPath();

            return new XmlTextWriter(path, Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                IndentChar = ' ',
                Indentation = 4
            };
        }
    }
}

#endif
