﻿using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json.Schema;

namespace GenerateFromSchema
{
    public class ValidationDocumentGenerator : Generator
    {
        private readonly string m_outputDirectory;

        public ValidationDocumentGenerator(string outputDirectory)
        {
            m_outputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory));
        }

        public override void Generate(Schema schema)
        {
            using (CodeWriter writer = new CodeWriter(Path.Combine(m_outputDirectory, "TestGenerateValidationDocument.cs")))
            {
                writer.WriteLine("// This file was generated automatically by GenerateFromSchema.  Do NOT edit it.");
                writer.WriteLine("// https://github.com/AnalyticalGraphicsInc/czml-writer");
                writer.WriteLine();

                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using System.Diagnostics.CodeAnalysis;");
                writer.WriteLine("using System.Drawing;");
                writer.WriteLine("using System.IO;");
                writer.WriteLine("using CesiumLanguageWriter;");
                writer.WriteLine("using NUnit.Framework;");
                writer.WriteLine();

                writer.WriteLine("namespace CesiumLanguageWriterTests");

                using (writer.OpenScope())
                {
                    writer.WriteLine("[TestFixture]");
                    writer.WriteLine("[SuppressMessage(\"ReSharper\", \"MemberCanBeMadeStatic.Local\")]");
                    writer.WriteLine("public class TestGenerateValidationDocument");

                    using (writer.OpenScope())
                    {
                        writer.WriteLine("private readonly JulianDate m_documentStartDate = new GregorianDate(2016, 6, 17, 12, 0, 0).ToJulianDate();");
                        writer.WriteLine("private readonly JulianDate m_documentStopDate = new GregorianDate(2016, 6, 17, 13, 0, 0).ToJulianDate();");

                        writer.WriteLine("private TextWriter m_streamWriter;");
                        writer.WriteLine("private TextWriter m_assertionsWriter;");
                        writer.WriteLine("private TextWriter m_extensionsAssertionsWriter;");
                        writer.WriteLine("private CesiumOutputStream m_output;");
                        writer.WriteLine("private CesiumStreamWriter m_writer;");

                        writer.WriteLine("private void WriteAssertionBoth(string s)");
                        using (writer.OpenScope())
                        {
                            writer.WriteLine("m_assertionsWriter.WriteLine(s);");
                            writer.WriteLine("m_extensionsAssertionsWriter.WriteLine(s);");
                        }

                        writer.WriteLine("private static IList<T> CreateList<T>(T t1, T t2)");
                        using (writer.OpenScope())
                        {
                            writer.WriteLine("return new List<T> { t1, t2 };");
                        }

                        writer.WriteLine("private static IList<T> CreateList<T>(params T[] ts)");
                        using (writer.OpenScope())
                        {
                            writer.WriteLine("return ts;");
                        }

                        writer.WriteLine("[Test]");
                        writer.WriteLine("public void GenerateValidationDocument()");

                        var schemaProperties = schema.Properties
                                                     .Where(p => p.Name != "clock" && p.Name != "availability" && !p.ValueType.IsSchemaFromType)
                                                     .Where(p => p.Name != "properties")
                                                     .ToList();

                        using (writer.OpenScope())
                        {
                            writer.WriteLine("using (m_streamWriter = new StreamWriter(\"ValidationDocument.czml\"))");
                            writer.WriteLine("using (m_assertionsWriter = new StreamWriter(\"ValidationDocumentAssertions.js\"))");
                            writer.WriteLine("using (m_extensionsAssertionsWriter = new StreamWriter(\"ValidationDocumentExtensionAssertions.js\"))");
                            using (writer.OpenScope())
                            {
                                WriteAssertionBoth(writer, "return CzmlDataSource.load('Data/CZML/ValidationDocument.czml').then(function(dataSource) {");
                                s_assertionIndent++;
                                WriteAssertionBoth(writer, "var e;");
                                WriteAssertionBoth(writer, "var date;");
                                WriteAssertionBoth(writer, "var documentStartDate = JulianDate.fromIso8601('2016-06-17T12:00:00Z');");
                                WriteAssertionBoth(writer, "var documentStopDate = JulianDate.fromIso8601('2016-06-17T13:00:00Z');");

                                writer.WriteLine("m_output = new CesiumOutputStream(m_streamWriter) { PrettyFormatting = true };");
                                writer.WriteLine("m_writer = new CesiumStreamWriter();");

                                writer.WriteLine("m_output.WriteStartSequence();");

                                // Java has a limit on the size of a single method (65535 bytes) so we have to break up all this code in multiple methods

                                writer.WriteLine("WriteClock();");
                                writer.WriteLine("WriteConstantValues();");
                                writer.WriteLine("WriteReferenceValues();");
                                writer.WriteLine("WriteSampledValues();");

                                writer.WriteLine("m_output.WriteEndSequence();");

                                s_assertionIndent--;
                                WriteAssertionBoth(writer, "});");
                                s_assertionIndent++;
                            }
                        }

                        writer.WriteLine("private void WriteClock()");
                        using (writer.OpenScope())
                        {
                            writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                            using (writer.OpenScope())
                            {
                                writer.WriteLine("packet.WriteId(\"document\");");
                                writer.WriteLine("packet.WriteName(\"ValidationDocument\");");
                                writer.WriteLine("packet.WriteVersion(\"1.0\");");

                                writer.WriteLine("using (var clock = packet.OpenClockProperty())");
                                using (writer.OpenScope())
                                {
                                    writer.WriteLine("clock.WriteInterval(m_documentStartDate, m_documentStopDate);");
                                    WriteAssertion(writer, false, "expect(dataSource.clock.startTime).toEqual(documentStartDate);");
                                    WriteAssertion(writer, false, "expect(dataSource.clock.stopTime).toEqual(documentStopDate);");

                                    writer.WriteLine("clock.WriteCurrentTime(m_documentStartDate);");
                                    WriteAssertion(writer, false, "expect(dataSource.clock.currentTime).toEqual(documentStartDate);");

                                    writer.WriteLine("clock.WriteMultiplier(1.0);");
                                    WriteAssertion(writer, false, "expect(dataSource.clock.multiplier).toEqual(1.0);");

                                    writer.WriteLine("clock.WriteRange(ClockRange.Unbounded);");
                                    WriteAssertion(writer, false, "expect(dataSource.clock.clockRange).toEqual(ClockRange.UNBOUNDED);");

                                    writer.WriteLine("clock.WriteStep(ClockStep.SystemClockMultiplier);");
                                    WriteAssertion(writer, false, "expect(dataSource.clock.clockStep).toEqual(ClockStep.SYSTEM_CLOCK_MULTIPLIER);");
                                }
                            }
                        }

                        writer.WriteLine("private void WriteConstantValues()");
                        using (writer.OpenScope())
                        {
                            writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                            using (writer.OpenScope())
                            {
                                const string id = "Constant";
                                writer.WriteLine("packet.WriteId(\"{0}\");", id);

                                WriteAssertionBoth(writer, "var constant = e = dataSource.entities.getById('Constant');");
                                WriteAssertionBoth(writer, "expect(e).toBeDefined();");
                                WriteAssertionBoth(writer, "date = JulianDate.now();");

                                // Write one packet with first value types for each property

                                foreach (var property in schemaProperties)
                                {
                                    string propertyName = property.Name;

                                    bool isExtension = !string.IsNullOrEmpty(property.ValueType.ExtensionPrefix);
                                    if (isExtension)
                                        propertyName = propertyName.Substring(property.ValueType.ExtensionPrefix.Length + 1);

                                    writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                    using (writer.OpenScope())
                                    {
                                        var properties = property.ValueType.Properties;

                                        var firstValueProperty = properties.FirstOrDefault(p => p.IsValue);
                                        if (firstValueProperty != null)
                                        {
                                            WriteValue(writer, "w", id, firstValueProperty, property, isExtension, propertyName);
                                        }
                                        else
                                        {
                                            foreach (var subProperty in properties.Where(p => !p.IsValue))
                                            {
                                                writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                using (writer.OpenScope())
                                                {
                                                    properties = subProperty.ValueType.Properties;
                                                    firstValueProperty = properties.FirstOrDefault(p => p.IsValue);

                                                    string subPropertyName = GetSubPropertyName(propertyName, subProperty);

                                                    if (firstValueProperty != null)
                                                    {
                                                        WriteValue(writer, "w2", id + property.Name, firstValueProperty, subProperty, isExtension, $"{propertyName}.{subPropertyName}");
                                                    }
                                                    else if (subProperty.ValueType.Name.Contains("Material"))
                                                    {
                                                        var materialProperty = properties.First();
                                                        writer.WriteLine("using (var m = w2.Open{0}Property())", materialProperty.NameWithPascalCase);
                                                        using (writer.OpenScope())
                                                        {
                                                            properties = materialProperty.ValueType.Properties;
                                                            foreach (var materialSubProperty in properties.Where(p => !p.IsValue))
                                                            {
                                                                writer.WriteLine("using (var m2 = m.Open{0}Property())", materialSubProperty.NameWithPascalCase);
                                                                using (writer.OpenScope())
                                                                {
                                                                    properties = materialSubProperty.ValueType.Properties;
                                                                    firstValueProperty = properties.FirstOrDefault(p => p.IsValue);
                                                                    if (firstValueProperty != null)
                                                                    {
                                                                        WriteValue(writer, "m2", id + property.Name + subProperty.Name + materialProperty.Name, firstValueProperty, materialSubProperty, isExtension, $"{propertyName}.{subPropertyName}.{materialSubProperty.Name}");
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var additionalProperties = subProperty.ValueType.AdditionalProperties;
                                                        if (additionalProperties != null)
                                                        {
                                                            writer.WriteLine("using (var a = w2.Open{0}Property(\"prop\"))", additionalProperties.ValueType.NameWithPascalCase);
                                                            using (writer.OpenScope())
                                                            {
                                                                foreach (var additionalProperty in additionalProperties.ValueType.Properties.Where(p => !p.IsValue))
                                                                {
                                                                    writer.WriteLine("using (var w3 = a.Open{0}Property())", additionalProperty.NameWithPascalCase);
                                                                    using (writer.OpenScope())
                                                                    {
                                                                        properties = additionalProperty.ValueType.Properties;
                                                                        firstValueProperty = properties.FirstOrDefault(p => p.IsValue);
                                                                        if (firstValueProperty != null)
                                                                        {
                                                                            WriteValue(writer, "w3", id + property.Name, firstValueProperty, additionalProperty, isExtension, $"{propertyName}.{subPropertyName}.prop.{additionalProperty.Name}");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            writer.WriteLine("WriteConstantValuesIndividual();");
                            writer.WriteLine("WriteConstantValuesCustomProperties();");
                        }

                        writer.WriteLine("private void WriteConstantValuesIndividual()");
                        using (writer.OpenScope())
                        {
                            // write other value types as individual packets
                            foreach (var property in schemaProperties)
                            {
                                writer.WriteLine("WriteConstantValuesIndividual{0}();", property.NameWithPascalCase);
                            }
                        }

                        foreach (var property in schemaProperties)
                        {
                            writer.WriteLine("private void WriteConstantValuesIndividual{0}()", property.NameWithPascalCase);
                            using (writer.OpenScope())
                            {
                                string propertyName = property.Name;

                                bool isExtension = !string.IsNullOrEmpty(property.ValueType.ExtensionPrefix);
                                if (isExtension)
                                    propertyName = propertyName.Substring(property.ValueType.ExtensionPrefix.Length + 1);

                                var properties = property.ValueType.Properties;
                                foreach (var valueProperty in properties.Where(p => p.IsValue && !p.ValueType.Name.StartsWith("Reference")).Skip(1))
                                {
                                    writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                    using (writer.OpenScope())
                                    {
                                        string id = $"constant_{property.Name}_{valueProperty.Name}";
                                        writer.WriteLine("packet.WriteId(\"{0}\");", id);

                                        WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                        writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                        using (writer.OpenScope())
                                        {
                                            WriteValue(writer, "w", id, valueProperty, property, isExtension, propertyName);
                                        }
                                    }
                                }

                                foreach (var subProperty in properties.Where(p => !p.IsValue))
                                {
                                    string subPropertyName = GetSubPropertyName(propertyName, subProperty);

                                    properties = subProperty.ValueType.Properties;

                                    foreach (var valueProperty in properties.Where(p => p.IsValue && !p.ValueType.Name.StartsWith("Reference")).Skip(1))
                                    {
                                        writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                        using (writer.OpenScope())
                                        {
                                            string id = $"constant_{property.Name}_{subPropertyName}_{valueProperty.Name}";
                                            writer.WriteLine("packet.WriteId(\"{0}\");", id);

                                            WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                            writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                            writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                            using (writer.OpenScope())
                                            {
                                                WriteValue(writer, "w2", id + property.Name, valueProperty, subProperty, isExtension, $"{propertyName}.{subPropertyName}");
                                            }
                                        }
                                    }

                                    if (subProperty.ValueType.Name.Contains("Material"))
                                    {
                                        // write other values for the first material
                                        // then write first values for other materials 
                                        // then write remaining values for other materials individually

                                        var firstMaterialProperty = properties.First();
                                        properties = firstMaterialProperty.ValueType.Properties;
                                        foreach (var materialSubProperty in properties.Where(p => !p.IsValue))
                                        {
                                            properties = materialSubProperty.ValueType.Properties;
                                            foreach (var valueProperty in properties.Where(p => p.IsValue && !p.ValueType.Name.StartsWith("Reference")).Skip(1))
                                            {
                                                writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                                using (writer.OpenScope())
                                                {
                                                    string id = $"constant_{propertyName}_{subPropertyName}_{firstMaterialProperty.Name}_{materialSubProperty.Name}";
                                                    writer.WriteLine("packet.WriteId(\"{0}\");", id);

                                                    WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                                    writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                                    writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                    writer.WriteLine("using (var m = w2.Open{0}Property())", firstMaterialProperty.NameWithPascalCase);
                                                    writer.WriteLine("using (var m2 = m.Open{0}Property())", materialSubProperty.NameWithPascalCase);
                                                    using (writer.OpenScope())
                                                    {
                                                        WriteValue(writer, "m2", id + property.Name + subProperty.Name + firstMaterialProperty.Name, valueProperty, materialSubProperty, isExtension, $"{propertyName}.{subPropertyName}.{materialSubProperty.Name}");
                                                    }
                                                }
                                            }
                                        }

                                        properties = subProperty.ValueType.Properties;
                                        foreach (var materialProperty in properties.Skip(1))
                                        {
                                            writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                            using (writer.OpenScope())
                                            {
                                                string id = $"material_{propertyName}_{subProperty.Name}_{materialProperty.Name}";
                                                writer.WriteLine("packet.WriteId(\"{0}\");", id);

                                                WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                                writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                                writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                writer.WriteLine("using (var m = w2.Open{0}Property())", materialProperty.NameWithPascalCase);
                                                using (writer.OpenScope())
                                                {
                                                    properties = materialProperty.ValueType.Properties;
                                                    foreach (var materialSubProperty in properties.Where(p => !p.IsValue))
                                                    {
                                                        writer.WriteLine("using (var m2 = m.Open{0}Property())", materialSubProperty.NameWithPascalCase);
                                                        using (writer.OpenScope())
                                                        {
                                                            properties = materialSubProperty.ValueType.Properties;
                                                            var firstValueProperty = properties.FirstOrDefault(p => p.IsValue);
                                                            if (firstValueProperty != null)
                                                            {
                                                                WriteValue(writer, "m2", id + property.Name + subProperty.Name + materialProperty.Name, firstValueProperty, materialSubProperty, isExtension, $"{propertyName}.{subPropertyName}.{materialSubProperty.Name}");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        properties = subProperty.ValueType.Properties;
                                        foreach (var materialProperty in properties.Skip(1))
                                        {
                                            properties = materialProperty.ValueType.Properties;
                                            foreach (var materialSubProperty in properties.Where(p => !p.IsValue))
                                            {
                                                properties = materialSubProperty.ValueType.Properties;
                                                foreach (var valueProperty in properties.Where(p => p.IsValue && !p.ValueType.Name.StartsWith("Reference")).Skip(1))
                                                {
                                                    writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                                    using (writer.OpenScope())
                                                    {
                                                        string id = $"constant_{propertyName}_{subPropertyName}_{materialProperty.Name}_{materialSubProperty.Name}";
                                                        writer.WriteLine("packet.WriteId(\"{0}\");", id);

                                                        WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                                        writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                                        writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                        writer.WriteLine("using (var m = w2.Open{0}Property())", materialProperty.NameWithPascalCase);
                                                        writer.WriteLine("using (var m2 = m.Open{0}Property())", materialSubProperty.NameWithPascalCase);
                                                        using (writer.OpenScope())
                                                        {
                                                            WriteValue(writer, "m2", id + property.Name + subProperty.Name + materialProperty.Name, valueProperty, materialSubProperty, isExtension, $"{propertyName}.{subPropertyName}.{materialSubProperty.Name}");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    var additionalProperties = subProperty.ValueType.AdditionalProperties;
                                    if (additionalProperties != null)
                                    {
                                        foreach (var additionalProperty in additionalProperties.ValueType.Properties.Where(p => !p.IsValue))
                                        {
                                            int i = 0;
                                            properties = additionalProperty.ValueType.Properties;
                                            foreach (var valueProperty in properties.Where(p => p.IsValue && !p.ValueType.Name.StartsWith("Reference")).Skip(1))
                                            {
                                                writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                                using (writer.OpenScope())
                                                {
                                                    string id = $"constant_{propertyName}_{subPropertyName}_{additionalProperties.ValueType.Name}_{additionalProperty.Name}";
                                                    writer.WriteLine("packet.WriteId(\"{0}\");", id);

                                                    WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                                    string propName = $"prop{i++}";
                                                    writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                                    writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                    writer.WriteLine("using (var a = w2.Open{0}Property(\"{1}\"))", additionalProperties.ValueType.NameWithPascalCase, propName);
                                                    writer.WriteLine("using (var w3 = a.Open{0}Property())", additionalProperty.NameWithPascalCase);
                                                    using (writer.OpenScope())
                                                    {
                                                        WriteValue(writer, "w3", id + property.Name + subProperty.Name, valueProperty, additionalProperty, isExtension, $"{propertyName}.{subPropertyName}.{propName}.{additionalProperty.Name}");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // create all types of custom properties
                        writer.WriteLine("private void WriteConstantValuesCustomProperties()");
                        using (writer.OpenScope())
                        {
                            var propertiesProperty = schema.Properties.FirstOrDefault(p => p.Name == "properties");
                            var additionalProperties = propertiesProperty?.ValueType.AdditionalProperties;
                            if (additionalProperties != null)
                            {
                                writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                using (writer.OpenScope())
                                {
                                    string id = "constant_custom";
                                    writer.WriteLine("packet.WriteId(\"{0}\");", id);
                                    WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                    writer.WriteLine("using (var w = packet.Open{0}Property())", propertiesProperty.NameWithPascalCase);
                                    using (writer.OpenScope())
                                    {
                                        foreach (var valueProperty in additionalProperties.ValueType.Properties.Where(p => p.IsValue))
                                        {
                                            string propName = $"custom_{valueProperty.Name}";
                                            writer.WriteLine("using (var w2 = w.Open{0}Property(\"{1}\"))", additionalProperties.ValueType.NameWithPascalCase, propName);
                                            using (writer.OpenScope())
                                            {
                                                WriteValue(writer, "w2", id + valueProperty.Name, valueProperty, additionalProperties, false, $"{propertiesProperty.Name}.{propName}");
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // create entities using references
                        writer.WriteLine("private void WriteReferenceValues()");
                        using (writer.OpenScope())
                        {
                            // write some positions and double values to use to create reference lists for position lists and double lists later

                            for (int i = 1; i <= 2; ++i)
                            {
                                writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                using (writer.OpenScope())
                                {
                                    string id = $"ConstantPosition{i}";
                                    writer.WriteLine("packet.WriteId(\"{0}\");", id);
                                    WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('ConstantPosition{0}')).toBeDefined();", i);
                                    var property = schemaProperties.First(p => p.Name == "position");
                                    writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                    using (writer.OpenScope())
                                    {
                                        var properties = property.ValueType.Properties;
                                        var firstValueProperty = properties.FirstOrDefault(p => p.IsValue);
                                        WriteValue(writer, "w", id, firstValueProperty, property, false, property.Name);
                                    }
                                }

                                writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                using (writer.OpenScope())
                                {
                                    string id = $"ConstantDouble{i}";
                                    writer.WriteLine("packet.WriteId(\"{0}\");", id);
                                    WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('ConstantDouble{0}')).toBeDefined();", i);
                                    var property = schemaProperties.First(p => p.Name == "billboard");
                                    var properties = property.ValueType.Properties;
                                    var subProperty = properties.First(p => p.Name == "scale");
                                    writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                    writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                    using (writer.OpenScope())
                                    {
                                        properties = subProperty.ValueType.Properties;
                                        var firstValueProperty = properties.FirstOrDefault(p => p.IsValue);
                                        WriteValue(writer, "w2", id + property.Name, firstValueProperty, subProperty, false, $"{property.Name}.{subProperty.Name}");
                                    }
                                }
                            }

                            // write one big packet with references for everything
                            writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                            using (writer.OpenScope())
                            {
                                writer.WriteLine("packet.WriteId(\"Reference\");");

                                WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('Reference')).toBeDefined();");

                                foreach (var property in schemaProperties)
                                {
                                    string propertyName = property.Name;

                                    bool isExtension = !string.IsNullOrEmpty(property.ValueType.ExtensionPrefix);
                                    if (isExtension)
                                        propertyName = propertyName.Substring(property.ValueType.ExtensionPrefix.Length + 1);

                                    writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                    using (writer.OpenScope())
                                    {
                                        var properties = property.ValueType.Properties;
                                        if (properties.Any(p => p.ValueType.Name == "Reference"))
                                        {
                                            writer.WriteLine("w.WriteReference(new Reference(\"Constant\", \"{0}\"));", propertyName);
                                            WriteAssertion(writer, isExtension, "expect(e.{0}.getValue(date)).toEqual(constant.{0}.getValue(date));", propertyName);
                                        }
                                        else
                                        {
                                            foreach (var subProperty in properties.Where(p => !p.IsValue))
                                            {
                                                properties = subProperty.ValueType.Properties;

                                                string subPropertyName = GetSubPropertyName(propertyName, subProperty);

                                                if (properties.Any(p => p.ValueType.Name == "Reference"))
                                                {
                                                    writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                    using (writer.OpenScope())
                                                    {
                                                        writer.WriteLine("w2.WriteReference(new Reference(\"Constant\", CreateList(\"{0}\", \"{1}\")));", propertyName, subPropertyName);
                                                        WriteAssertion(writer, isExtension, "expect(e.{0}.{1}.getValue(date)).toEqual(constant.{0}.{1}.getValue(date));", propertyName, subPropertyName);
                                                    }
                                                }
                                                else if (properties.Any(p => p.ValueType.Name == "ReferenceList"))
                                                {
                                                    string targetId = "";
                                                    string[] referencePropertyNames = { };
                                                    if (subProperty.ValueType.Name == "PositionList")
                                                    {
                                                        targetId = "Position";
                                                        referencePropertyNames = new[] { "position" };
                                                    }
                                                    else if (subProperty.ValueType.Name == "DoubleList")
                                                    {
                                                        targetId = "Double";
                                                        referencePropertyNames = new[] { "billboard", "scale" };
                                                    }

                                                    writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                    using (writer.OpenScope())
                                                    {
                                                        writer.WriteLine("w2.WriteReferences(CreateList(new Reference(\"Constant{0}1\", CreateList({1})), new Reference(\"Constant{0}2\", CreateList({1}))));", targetId, string.Join(", ", referencePropertyNames.Select(n => $"\"{n}\"")));
                                                        WriteAssertion(writer, isExtension, "expect(e.{0}.{1}.getValue(date)).toEqual([dataSource.entities.getById('Constant{2}1').{3}.getValue(date), dataSource.entities.getById('Constant{2}2').{3}.getValue(date)]);", propertyName, subPropertyName, targetId, string.Join(".", referencePropertyNames));
                                                    }
                                                }
                                                else if (subProperty.ValueType.Name.Contains("Material"))
                                                {
                                                    Property materialProperty = properties.First();
                                                    writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                    writer.WriteLine("using (var m = w2.Open{0}Property())", materialProperty.NameWithPascalCase);
                                                    using (writer.OpenScope())
                                                    {
                                                        properties = materialProperty.ValueType.Properties;
                                                        foreach (var materialSubProperty in properties.Where(p => !p.IsValue))
                                                        {
                                                            writer.WriteLine("using (var m2 = m.Open{0}Property())", materialSubProperty.NameWithPascalCase);
                                                            using (writer.OpenScope())
                                                            {
                                                                writer.WriteLine("m2.WriteReference(new Reference(\"Constant\", CreateList(\"{0}\", \"{1}\", \"{2}\")));", propertyName, subPropertyName, materialSubProperty.Name);
                                                                WriteAssertion(writer, isExtension, "expect(e.{0}.{1}.{2}.getValue(date)).toEqual(constant.{0}.{1}.{2}.getValue(date));", propertyName, subPropertyName, materialSubProperty.Name);
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var additionalProperties = subProperty.ValueType.AdditionalProperties;
                                                    if (additionalProperties != null)
                                                    {
                                                        writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                        writer.WriteLine("using (var a = w2.Open{0}Property(\"referenceProp\"))", additionalProperties.ValueType.NameWithPascalCase);
                                                        using (writer.OpenScope())
                                                        {
                                                            foreach (var additionalProperty in additionalProperties.ValueType.Properties.Where(p => !p.IsValue))
                                                            {
                                                                writer.WriteLine("using (var w3 = a.Open{0}Property())", additionalProperty.NameWithPascalCase);
                                                                using (writer.OpenScope())
                                                                {
                                                                    writer.WriteLine("w3.WriteReference(new Reference(\"Constant\", CreateList(\"{0}\", \"{1}\", \"prop\", \"{2}\")));", propertyName, subPropertyName, additionalProperty.Name);
                                                                    WriteAssertion(writer, isExtension, "expect(e.{0}.{1}.referenceProp.{2}.getValue(date)).toEqual(constant.{0}.{1}.prop.{2}.getValue(date));", propertyName, subPropertyName, additionalProperty.Name);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            writer.WriteLine("WriteReferenceValuesIndividual();");
                        }

                        writer.WriteLine("private void WriteReferenceValuesIndividual()");
                        using (writer.OpenScope())
                        {
                            // write other materials as individual packets
                            foreach (var property in schemaProperties)
                            {
                                writer.WriteLine("WriteReferenceValuesIndividual{0}();", property.NameWithPascalCase);
                            }

                            // special case - velocity reference
                            writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                            using (writer.OpenScope())
                            {
                                writer.WriteLine("packet.WriteId(\"VelocityPosition\");");
                                WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('VelocityPosition')).toBeDefined();");
                                var property = schemaProperties.First(p => p.Name == "position");
                                writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                using (writer.OpenScope())
                                {
                                    // construct a position with specific velocity
                                    double dx = 1.0;
                                    double dy = 2.0;
                                    double dz = 3.0;
                                    double deltaTime = 60.0;

                                    double x1 = 1.0;
                                    double x2 = x1 + dx * deltaTime;

                                    double y1 = 2.0;
                                    double y2 = y1 + dy * deltaTime;

                                    double z1 = 3.0;
                                    double z2 = z1 + dz * deltaTime;

                                    string value1 = string.Format("new Cartesian({0}, {1}, {2})", x1, y1, z1);
                                    string assertionValue1 = string.Format("new Cartesian3({0}, {1}, {2})", x1, y1, z1);

                                    string value2 = string.Format("new Cartesian({0}, {1}, {2})", x2, y2, z2);
                                    string assertionValue2 = string.Format("new Cartesian3({0}, {1}, {2})", x2, y2, z2);

                                    writer.WriteLine("w.WriteCartesian(CreateList(m_documentStartDate, m_documentStartDate.AddSeconds({0})), CreateList({1}, {2}));", deltaTime, value1, value2);
                                    WriteAssertion(writer, false, "expect(e.position.getValue(documentStartDate)).toEqual({0});", assertionValue1);
                                    WriteAssertion(writer, false, "expect(e.position.getValue(JulianDate.addSeconds(documentStartDate, {0}, new JulianDate()))).toEqual({1});", deltaTime, assertionValue2);
                                }
                            }

                            foreach (var property in schemaProperties)
                            {
                                string propertyName = property.Name;

                                bool isExtension = !string.IsNullOrEmpty(property.ValueType.ExtensionPrefix);
                                if (isExtension)
                                    propertyName = propertyName.Substring(property.ValueType.ExtensionPrefix.Length + 1);

                                var properties = property.ValueType.Properties;

                                foreach (var subProperty in properties.Where(p => !p.IsValue))
                                {
                                    properties = subProperty.ValueType.Properties;
                                    if (properties.Any(p => p.Name == "velocityReference"))
                                    {
                                        writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                        using (writer.OpenScope())
                                        {
                                            string id = $"velocityReference_{propertyName}_{subProperty.Name}";
                                            writer.WriteLine("packet.WriteId(\"{0}\");", id);

                                            WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                            writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                            writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                            using (writer.OpenScope())
                                            {
                                                writer.WriteLine("w2.WriteVelocityReference(new Reference(\"VelocityPosition\", \"position\"));");

                                                double expectedX = 1.0;
                                                double expectedY = 2.0;
                                                double expectedZ = 3.0;
                                                bool isNormalized = subProperty.NameWithPascalCase == "AlignedAxis";
                                                if (isNormalized)
                                                {
                                                    NormalizeCartesian(ref expectedX, ref expectedY, ref expectedZ);
                                                }

                                                WriteAssertion(writer, isExtension, "expect(e.{0}.{1}.getValue(JulianDate.addSeconds(documentStartDate, 50, new JulianDate()))).toEqualEpsilon(new Cartesian3({2}, {3}, {4}), 1e-13);", propertyName, subProperty.Name, expectedX, expectedY, expectedZ);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // write other materials as individual packets
                        foreach (var property in schemaProperties)
                        {
                            writer.WriteLine("private void WriteReferenceValuesIndividual{0}()", property.NameWithPascalCase);
                            using (writer.OpenScope())
                            {
                                string propertyName = property.Name;

                                bool isExtension = !string.IsNullOrEmpty(property.ValueType.ExtensionPrefix);
                                if (isExtension)
                                    propertyName = propertyName.Substring(property.ValueType.ExtensionPrefix.Length + 1);

                                var properties = property.ValueType.Properties;

                                foreach (var subProperty in properties.Where(p => !p.IsValue))
                                {
                                    string subPropertyName = GetSubPropertyName(propertyName, subProperty);

                                    if (subProperty.ValueType.Name.Contains("Material"))
                                    {
                                        properties = subProperty.ValueType.Properties;
                                        foreach (var materialProperty in properties.Skip(1))
                                        {
                                            writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                            using (writer.OpenScope())
                                            {
                                                string targetId = $"material_{propertyName}_{subProperty.Name}_{materialProperty.Name}";
                                                string id = $"reference_{propertyName}_{subPropertyName}_{materialProperty.Name}";

                                                writer.WriteLine("packet.WriteId(\"{0}\");", id);
                                                WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                                writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                                writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                writer.WriteLine("using (var m = w2.Open{0}Property())", materialProperty.NameWithPascalCase);
                                                using (writer.OpenScope())
                                                {
                                                    properties = materialProperty.ValueType.Properties;
                                                    foreach (var materialSubProperty in properties.Where(p => !p.IsValue))
                                                    {
                                                        writer.WriteLine("using (var m2 = m.Open{0}Property())", materialSubProperty.NameWithPascalCase);
                                                        using (writer.OpenScope())
                                                        {
                                                            writer.WriteLine("m2.WriteReference(new Reference(\"{0}\", CreateList(\"{1}\", \"{2}\", \"{3}\")));", targetId, propertyName, subPropertyName, materialSubProperty.Name);
                                                            WriteAssertion(writer, isExtension, "expect(e.{1}.{2}.{3}.getValue(date)).toEqual(dataSource.entities.getById('{0}').{1}.{2}.{3}.getValue(date));", targetId, propertyName, subPropertyName, materialSubProperty.Name);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // test sampled properties
                        writer.WriteLine("private void WriteSampledValues()");
                        using (writer.OpenScope())
                        {
                            // Write one packet with first value types for each property

                            writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                            using (writer.OpenScope())
                            {
                                const string id = "Sampled";
                                writer.WriteLine("packet.WriteId(\"{0}\");", id);
                                WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('Sampled')).toBeDefined();");

                                foreach (var property in schemaProperties)
                                {
                                    string propertyName = property.Name;

                                    bool isExtension = !string.IsNullOrEmpty(property.ValueType.ExtensionPrefix);
                                    if (isExtension)
                                        propertyName = propertyName.Substring(property.ValueType.ExtensionPrefix.Length + 1);

                                    var properties = property.ValueType.Properties;
                                    if (property.IsInterpolatable)
                                    {
                                        writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                        using (writer.OpenScope())
                                        {
                                            var firstValueProperty = properties.First(p => p.IsValue && (p.ValueType.JsonTypes & JsonSchemaType.Array) == JsonSchemaType.Array);
                                            WriteValues(writer, "w", id, firstValueProperty, property, isExtension, propertyName);
                                        }
                                    }
                                    else
                                    {
                                        var subProperties = properties.Where(p => !p.IsValue && (p.IsInterpolatable || p.ValueType.Name.Contains("Material"))).ToList();
                                        if (subProperties.Any())
                                        {
                                            writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                            using (writer.OpenScope())
                                            {
                                                foreach (var subProperty in subProperties)
                                                {
                                                    properties = subProperty.ValueType.Properties;

                                                    string subPropertyName = GetSubPropertyName(propertyName, subProperty);
                                                    writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                    using (writer.OpenScope())
                                                    {
                                                        if (subProperty.IsInterpolatable)
                                                        {
                                                            var firstValueProperty = properties.First(p => p.IsValue && (p.ValueType.JsonTypes & JsonSchemaType.Array) == JsonSchemaType.Array);
                                                            WriteValues(writer, "w2", id + property.Name, firstValueProperty, subProperty, isExtension, $"{propertyName}.{subPropertyName}");
                                                        }
                                                        else if (subProperty.ValueType.Name.Contains("Material"))
                                                        {
                                                            Property materialProperty = properties.First();
                                                            writer.WriteLine("using (var m = w2.Open{0}Property())", materialProperty.NameWithPascalCase);
                                                            using (writer.OpenScope())
                                                            {
                                                                properties = materialProperty.ValueType.Properties;
                                                                foreach (var materialSubProperty in properties.Where(p => p.IsInterpolatable))
                                                                {
                                                                    writer.WriteLine("using (var m2 = m.Open{0}Property())", materialSubProperty.NameWithPascalCase);
                                                                    using (writer.OpenScope())
                                                                    {
                                                                        properties = materialSubProperty.ValueType.Properties;
                                                                        var firstValueProperty = properties.First(p => p.IsValue && (p.ValueType.JsonTypes & JsonSchemaType.Array) == JsonSchemaType.Array);
                                                                        WriteValues(writer, "m2", id + property.Name + subProperty.Name + materialProperty.Name, firstValueProperty, materialSubProperty, isExtension, $"{propertyName}.{subPropertyName}.{materialSubProperty.Name}");
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            writer.WriteLine("WriteSampledValuesIndividual();");
                            writer.WriteLine("WriteSampledValuesCustomProperties();");
                        }

                        writer.WriteLine("private void WriteSampledValuesIndividual()");
                        using (writer.OpenScope())
                        {
                            // write other value types as individual packets

                            // split method into multiple methods
                            foreach (var property in schemaProperties)
                            {
                                writer.WriteLine("WriteSampledValuesIndividual{0}();", property.NameWithPascalCase);
                            }
                        }

                        foreach (var property in schemaProperties)
                        {
                            writer.WriteLine("private void WriteSampledValuesIndividual{0}()", property.NameWithPascalCase);
                            using (writer.OpenScope())
                            {
                                string propertyName = property.Name;

                                bool isExtension = !string.IsNullOrEmpty(property.ValueType.ExtensionPrefix);
                                if (isExtension)
                                    propertyName = propertyName.Substring(property.ValueType.ExtensionPrefix.Length + 1);

                                var properties = property.ValueType.Properties;

                                if (property.IsInterpolatable)
                                {
                                    foreach (var valueProperty in properties.Where(p => p.IsValue && (p.ValueType.JsonTypes & JsonSchemaType.Array) == JsonSchemaType.Array).Skip(1))
                                    {
                                        writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                        using (writer.OpenScope())
                                        {
                                            string id = $"sampled_{propertyName}_{valueProperty.Name}";
                                            writer.WriteLine("packet.WriteId(\"{0}\");", id);

                                            WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                            writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                            using (writer.OpenScope())
                                            {
                                                WriteValues(writer, "w", id, valueProperty, property, isExtension, propertyName);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var subProperty in properties.Where(p => !p.IsValue))
                                    {
                                        string subPropertyName = GetSubPropertyName(propertyName, subProperty);

                                        properties = subProperty.ValueType.Properties;

                                        if (subProperty.IsInterpolatable)
                                        {
                                            foreach (var valueProperty in properties.Where(p => p.IsValue && (p.ValueType.JsonTypes & JsonSchemaType.Array) == JsonSchemaType.Array).Skip(1))
                                            {
                                                writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                                using (writer.OpenScope())
                                                {
                                                    string id = $"sampled_{propertyName}_{subPropertyName}_{valueProperty.Name}";
                                                    writer.WriteLine("packet.WriteId(\"{0}\");", id);

                                                    WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                                    writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                                    writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                    using (writer.OpenScope())
                                                    {
                                                        WriteValues(writer, "w2", id + property.Name, valueProperty, subProperty, isExtension, $"{propertyName}.{subPropertyName}");
                                                    }
                                                }
                                            }
                                        }
                                        else if (subProperty.ValueType.Name.Contains("Material"))
                                        {
                                            // write other values for the first material
                                            // then write first values for other materials 
                                            // then write remaining values for other materials individually

                                            var firstMaterialProperty = properties.First();
                                            properties = firstMaterialProperty.ValueType.Properties;
                                            foreach (var materialSubProperty in properties.Where(p => p.IsInterpolatable))
                                            {
                                                properties = materialSubProperty.ValueType.Properties;
                                                foreach (var valueProperty in properties.Where(p => p.IsValue && (p.ValueType.JsonTypes & JsonSchemaType.Array) == JsonSchemaType.Array).Skip(1))
                                                {
                                                    writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                                    using (writer.OpenScope())
                                                    {
                                                        string id = $"sampled_{propertyName}_{subPropertyName}_{firstMaterialProperty.Name}_{materialSubProperty.Name}";
                                                        writer.WriteLine("packet.WriteId(\"{0}\");", id);

                                                        WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                                        writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                                        writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                        writer.WriteLine("using (var m = w2.Open{0}Property())", firstMaterialProperty.NameWithPascalCase);
                                                        writer.WriteLine("using (var m2 = m.Open{0}Property())", materialSubProperty.NameWithPascalCase);
                                                        using (writer.OpenScope())
                                                        {
                                                            WriteValues(writer, "m2", id + property.Name + subProperty.Name + firstMaterialProperty.Name, valueProperty, materialSubProperty, isExtension, $"{propertyName}.{subPropertyName}.{materialSubProperty.Name}");
                                                        }
                                                    }
                                                }
                                            }

                                            properties = subProperty.ValueType.Properties;
                                            foreach (var materialProperty in properties.Skip(1))
                                            {
                                                writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                                using (writer.OpenScope())
                                                {
                                                    string id = $"sampled_{propertyName}_{subPropertyName}_{materialProperty.Name}";
                                                    writer.WriteLine("packet.WriteId(\"{0}\");", id);

                                                    WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                                    writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                                    writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                    writer.WriteLine("using (var m = w2.Open{0}Property())", materialProperty.NameWithPascalCase);
                                                    using (writer.OpenScope())
                                                    {
                                                        properties = materialProperty.ValueType.Properties;
                                                        foreach (var materialSubProperty in properties.Where(p => p.IsInterpolatable))
                                                        {
                                                            writer.WriteLine("using (var m2 = m.Open{0}Property())", materialSubProperty.NameWithPascalCase);
                                                            using (writer.OpenScope())
                                                            {
                                                                properties = materialSubProperty.ValueType.Properties;
                                                                var firstValueProperty = properties.First(p => p.IsValue && (p.ValueType.JsonTypes & JsonSchemaType.Array) == JsonSchemaType.Array);
                                                                WriteValues(writer, "m2", id + property.Name + subProperty.Name + materialProperty.Name, firstValueProperty, materialSubProperty, isExtension, $"{propertyName}.{subPropertyName}.{materialSubProperty.Name}");
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            properties = subProperty.ValueType.Properties;
                                            foreach (var materialProperty in properties.Skip(1))
                                            {
                                                properties = materialProperty.ValueType.Properties;
                                                foreach (var materialSubProperty in properties.Where(p => p.IsInterpolatable))
                                                {
                                                    properties = materialSubProperty.ValueType.Properties;
                                                    foreach (var valueProperty in properties.Where(p => p.IsValue && (p.ValueType.JsonTypes & JsonSchemaType.Array) == JsonSchemaType.Array).Skip(1))
                                                    {
                                                        writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                                        using (writer.OpenScope())
                                                        {
                                                            string id = $"sampled_{propertyName}_{subPropertyName}_{materialProperty.Name}_{materialSubProperty.Name}";
                                                            writer.WriteLine("packet.WriteId(\"{0}\");", id);

                                                            WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                                            writer.WriteLine("using (var w = packet.Open{0}Property())", property.NameWithPascalCase);
                                                            writer.WriteLine("using (var w2 = w.Open{0}Property())", subProperty.NameWithPascalCase);
                                                            writer.WriteLine("using (var m = w2.Open{0}Property())", materialProperty.NameWithPascalCase);
                                                            writer.WriteLine("using (var m2 = m.Open{0}Property())", materialSubProperty.NameWithPascalCase);
                                                            using (writer.OpenScope())
                                                            {
                                                                WriteValues(writer, "m2", id + property.Name + subProperty.Name + materialProperty.Name, valueProperty, materialSubProperty, isExtension, $"{propertyName}.{subPropertyName}.{materialSubProperty.Name}");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // create all types of custom properties
                        writer.WriteLine("private void WriteSampledValuesCustomProperties()");
                        using (writer.OpenScope())
                        {
                            var propertiesProperty = schema.Properties.FirstOrDefault(p => p.Name == "properties");
                            var additionalProperties = propertiesProperty?.ValueType.AdditionalProperties;
                            if (additionalProperties != null)
                            {
                                writer.WriteLine("using (var packet = m_writer.OpenPacket(m_output))");
                                using (writer.OpenScope())
                                {
                                    string id = "sampled_custom";
                                    writer.WriteLine("packet.WriteId(\"{0}\");", id);
                                    WriteAssertionBoth(writer, "expect(e = dataSource.entities.getById('{0}')).toBeDefined();", id);

                                    writer.WriteLine("using (var w = packet.Open{0}Property())", propertiesProperty.NameWithPascalCase);
                                    using (writer.OpenScope())
                                    {
                                        foreach (var valueProperty in additionalProperties.ValueType.Properties.Where(p => p.IsValue && (p.ValueType.JsonTypes & JsonSchemaType.Array) == JsonSchemaType.Array))
                                        {
                                            string propName = $"custom_{valueProperty.Name}";
                                            writer.WriteLine("using (var w2 = w.Open{0}Property(\"{1}\"))", additionalProperties.ValueType.NameWithPascalCase, propName);
                                            using (writer.OpenScope())
                                            {
                                                WriteValues(writer, "w2", id + valueProperty.Name, valueProperty, additionalProperties, false, $"{propertiesProperty.Name}.{propName}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string GetSubPropertyName(string propertyName, Property subProperty)
        {
            string subPropertyName = subProperty.Name;
            if (subPropertyName == "gltf")
                subPropertyName = "uri";
            else if (propertyName == "polygon" && subPropertyName == "positions")
                subPropertyName = "hierarchy";
            return subPropertyName;
        }

        private static void WriteValue(CodeWriter writer, string openWriterName, string valueName, Property valueProperty, Property parentProperty, bool isExtension, string propertyName)
        {
            GetUniqueValue(valueName, valueProperty, parentProperty, out string value, out string valueSuffix, out string assertionValue, out string assertionEpsilon, out _);
            writer.WriteLine("{0}.Write{1}({2});", openWriterName, valueProperty.NameWithPascalCase, value);
            WriteAssertion(writer, isExtension, "expect(e.{0}.getValue(date){1}).{2}({3}{4});",
                           propertyName,
                           valueSuffix,
                           assertionEpsilon == null ? "toEqual" : "toEqualEpsilon",
                           assertionValue,
                           assertionEpsilon == null ? "" : $", {assertionEpsilon}");
        }

        private static void WriteValues(CodeWriter writer, string openWriterName, string valueName, Property valueProperty, Property parentProperty, bool isExtension, string propertyName)
        {
            GetUniqueValue(valueName + 1, valueProperty, parentProperty, out string value1, out string valueSuffix1, out string assertionValue1, out string assertionEpsilon1, out _);
            GetUniqueValue(valueName + 2, valueProperty, parentProperty, out string value2, out string valueSuffix2, out string assertionValue2, out string assertionEpsilon2, out _);
            writer.WriteLine("{0}.Write{1}(CreateList(m_documentStartDate, m_documentStopDate), CreateList({2}, {3}));", openWriterName, valueProperty.NameWithPascalCase, value1, value2);
            WriteAssertion(writer, isExtension, "expect(e.{0}.getValue(documentStartDate){1}).{2}({3}{4});",
                           propertyName,
                           valueSuffix1,
                           assertionEpsilon1 == null ? "toEqual" : "toEqualEpsilon",
                           assertionValue1,
                           assertionEpsilon1 == null ? "" : $", {assertionEpsilon1}");
            WriteAssertion(writer, isExtension, "expect(e.{0}.getValue(documentStopDate){1}).toEqual{2}({3}{4});",
                           propertyName,
                           valueSuffix2,
                           assertionEpsilon2 == null ? "" : "Epsilon",
                           assertionValue2,
                           assertionEpsilon2 == null ? "" : $", {assertionEpsilon2}");
        }

        private static void GetUniqueValue(string valueName, Property valueProperty, Property parentProperty,
                                           [NotNull] out string value, [NotNull] out string valueSuffix,
                                           [NotNull] out string assertionValue, out string assertionEpsilon,
                                           [NotNull] out string valueType)
        {
            value = valueSuffix = assertionValue = valueType = "";
            assertionEpsilon = null;

            var hash = new Lazy<byte[]>(() =>
            {
                using (var md5 = MD5.Create())
                {
                    return md5.ComputeHash(Encoding.UTF8.GetBytes(valueName + parentProperty.Name + valueProperty.Name));
                }
            });

            int GetNumber(int n)
            {
                return BitConverter.ToUInt16(hash.Value, n);
            }

            switch (valueProperty.ValueType.Name)
            {
                case "Boolean":
                {
                    value = assertionValue = "true";
                    valueType = "bool";
                    return;
                }
                case "Double":
                {
                    value = assertionValue = GetNumber(0).ToString("#.0");
                    valueType = "double";
                    return;
                }
                case "Integer":
                {
                    value = assertionValue = GetNumber(0).ToString();
                    valueType = "int";
                    return;
                }
                case "DoubleList":
                {
                    int v1 = GetNumber(0);
                    int v2 = GetNumber(1);
                    value = string.Format("CreateList<double>({0}, {1})", v1, v2);
                    assertionValue = string.Format("[ {0}, {1} ]", v1, v2);
                    valueType = "List<double>";
                    return;
                }
                case "String":
                {
                    string v = string.Format("string{0}", GetNumber(0));
                    value = string.Format("\"{0}\"", v);
                    assertionValue = string.Format("'{0}'", v);
                    valueType = "string";
                    return;
                }
                case "Cartesian3":
                {
                    int x = GetNumber(0);
                    int y = GetNumber(1);
                    int z = GetNumber(2);
                    value = string.Format("new Cartesian({0}, {1}, {2})", x, y, z);
                    assertionValue = string.Format("new Cartesian3({0}, {1}, {2})", x, y, z);
                    valueType = "Cartesian";
                    return;
                }
                case "Cartesian3List":
                {
                    int x1 = GetNumber(0);
                    int y1 = GetNumber(1);
                    int z1 = GetNumber(2);
                    int x2 = GetNumber(3);
                    int y2 = GetNumber(4);
                    int z2 = GetNumber(5);
                    value = string.Format("CreateList(new Cartesian({0}, {1}, {2}), new Cartesian({3}, {4}, {5}))", x1, y1, z1, x2, y2, z2);
                    assertionValue = string.Format("[ {6}new Cartesian3({0}, {1}, {2}){7}, {6}new Cartesian3({3}, {4}, {5}){7} ]", x1, y1, z1, x2, y2, z2,
                                                   parentProperty.ValueType.Name == "DirectionList" ? "Spherical.fromCartesian3(" : "",
                                                   parentProperty.ValueType.Name == "DirectionList" ? ")" : "");
                    valueType = "List<Cartesian>";
                    return;
                }
                case "UnitCartesian3":
                {
                    double x = GetNumber(0);
                    double y = GetNumber(1);
                    double z = GetNumber(2);
                    NormalizeCartesian(ref x, ref y, ref z);

                    value = string.Format("new UnitCartesian({0}, {1}, {2})", x, y, z);
                    assertionValue = string.Format("new Cartesian3({0}, {1}, {2})", x, y, z);
                    valueType = "UnitCartesian";
                    assertionEpsilon = "1e-14";
                    return;
                }
                case "UnitCartesian3List":
                {
                    double x1 = GetNumber(0);
                    double y1 = GetNumber(1);
                    double z1 = GetNumber(2);
                    NormalizeCartesian(ref x1, ref y1, ref z1);

                    double x2 = GetNumber(3);
                    double y2 = GetNumber(4);
                    double z2 = GetNumber(5);
                    NormalizeCartesian(ref x2, ref y2, ref z2);

                    value = string.Format("CreateList(new UnitCartesian({0}, {1}, {2}), new UnitCartesian({3}, {4}, {5}))", x1, y1, z1, x2, y2, z2);
                    assertionValue = string.Format("[ {6}new Cartesian3({0}, {1}, {2}){7}, {6}new Cartesian3({3}, {4}, {5}){7} ]", x1, y1, z1, x2, y2, z2,
                                                   parentProperty.ValueType.Name == "DirectionList" ? "Spherical.fromCartesian3(" : "",
                                                   parentProperty.ValueType.Name == "DirectionList" ? ")" : "");
                    assertionEpsilon = "1e-14";
                    valueType = "List<UnitCartesian>";
                    return;
                }
                case "Cartesian3Velocity":
                {
                    int x = GetNumber(0);
                    int y = GetNumber(1);
                    int z = GetNumber(2);
                    int dX = GetNumber(3);
                    int dY = GetNumber(4);
                    int dZ = GetNumber(5);
                    value = string.Format("new Motion<Cartesian>(new Cartesian({0}, {1}, {2}), new Cartesian({3}, {4}, {5}))", x, y, z, dX, dY, dZ);
                    assertionValue = string.Format("new Cartesian3({0}, {1}, {2})", x, y, z);
                    valueType = "Motion<Cartesian>";
                    return;
                }
                case "Cartographic":
                {
                    bool isDegrees = valueProperty.Name == "cartographicDegrees";
                    double longitude = GetNumber(0) % (isDegrees ? 45 : Math.PI / 2);
                    double latitude = GetNumber(1) % (isDegrees ? 45 : Math.PI / 2);
                    double height = GetNumber(2);
                    value = string.Format("new Cartographic({0}, {1}, {2})", longitude, latitude, height);
                    assertionValue = string.Format("Cartesian3.from{3}({0}, {1}, {2})", longitude, latitude, height,
                                                   isDegrees ? "Degrees" : "Radians");
                    valueType = "Cartographic";
                    return;
                }
                case "CartographicList":
                {
                    bool isDegrees = valueProperty.Name == "cartographicDegrees";
                    double longitude1 = GetNumber(0) % (isDegrees ? 45 : Math.PI / 2);
                    double latitude1 = GetNumber(1) % (isDegrees ? 45 : Math.PI / 2);
                    double height1 = GetNumber(2);
                    double longitude2 = GetNumber(3) % (isDegrees ? 45 : Math.PI / 2);
                    double latitude2 = GetNumber(4) % (isDegrees ? 45 : Math.PI / 2);
                    double height2 = GetNumber(5);
                    value = string.Format("CreateList(new Cartographic({0}, {1}, {2}), new Cartographic({3}, {4}, {5}))", longitude1, latitude1, height1, longitude2, latitude2, height2);
                    assertionValue = string.Format("[ Cartesian3.from{6}({0}, {1}, {2}), Cartesian3.from{6}({3}, {4}, {5}) ]", longitude1, latitude1, height1, longitude2, latitude2, height2,
                                                   isDegrees ? "Degrees" : "Radians");
                    valueType = "List<Cartesian>";
                    return;
                }
                case "Cartesian2":
                {
                    int x = GetNumber(0);
                    int y = GetNumber(1);
                    value = string.Format("new Rectangular({0}, {1})", x, y);
                    assertionValue = string.Format("new Cartesian2({0}, {1})", x, y);
                    valueType = "Rectangular";
                    return;
                }
                case "Spherical":
                {
                    int clock = GetNumber(0);
                    int cone = GetNumber(1);
                    int magnitude = GetNumber(2);
                    value = string.Format("new Spherical({0}, {1}, {2})", clock, cone, magnitude);
                    assertionValue = string.Format("Cartesian3.fromSpherical(new Spherical({0}, {1}, {2}))", clock, cone, magnitude);
                    valueType = "Spherical";
                    return;
                }
                case "SphericalList":
                {
                    int clock1 = GetNumber(0);
                    int cone1 = GetNumber(1);
                    int magnitude1 = GetNumber(2);
                    int clock2 = GetNumber(3);
                    int cone2 = GetNumber(4);
                    int magnitude2 = GetNumber(5);
                    value = string.Format("CreateList(new Spherical({0}, {1}, {2}), new Spherical({3}, {4}, {5}))", clock1, cone1, magnitude1, clock2, cone2, magnitude2);
                    assertionValue = string.Format("[ new Spherical({0}, {1}, {2}), new Spherical({3}, {4}, {5}) ]", clock1, cone1, magnitude1, clock2, cone2, magnitude2);
                    valueType = "List<Spherical>";
                    return;
                }
                case "UnitSpherical":
                {
                    int clock = GetNumber(0);
                    int cone = GetNumber(1);
                    value = string.Format("new UnitSpherical({0}, {1})", clock, cone);
                    assertionValue = string.Format("Cartesian3.fromSpherical(new Spherical({0}, {1}))", clock, cone);
                    assertionEpsilon = "1e-14";
                    valueType = "UnitSpherical";
                    return;
                }
                case "UnitSphericalList":
                {
                    int clock1 = GetNumber(0);
                    int cone1 = GetNumber(1);
                    int clock2 = GetNumber(2);
                    int cone2 = GetNumber(3);
                    value = string.Format("CreateList(new UnitSpherical({0}, {1}), new UnitSpherical({2}, {3}))", clock1, cone1, clock2, cone2);
                    assertionValue = string.Format("[ new Spherical({0}, {1}), new Spherical({2}, {3}) ]", clock1, cone1, clock2, cone2);
                    valueType = "List<UnitSpherical>";
                    return;
                }
                case "UnitQuaternion":
                {
                    double w = GetNumber(0);
                    double x = GetNumber(1);
                    double y = GetNumber(2);
                    double z = GetNumber(3);

                    double magnitude = Math.Sqrt(w * w + x * x + y * y + z * z);

                    w /= magnitude;
                    x /= magnitude;
                    y /= magnitude;
                    z /= magnitude;
                    value = string.Format("new UnitQuaternion({0}, {1}, {2}, {3})", w, x, y, z);
                    assertionValue = string.Format("new Quaternion({0}, {1}, {2}, {3})", x, y, z, w);
                    assertionEpsilon = "1e-14";
                    valueType = "UnitQuaternion";
                    return;
                }
                case "Rgba":
                {
                    int a = GetNumber(0) % 255;
                    int r = GetNumber(1) % 255;
                    int g = GetNumber(2) % 255;
                    int b = GetNumber(3) % 255;

                    value = string.Format("Color.FromArgb({0}, {1}, {2}, {3})", a, r, g, b);
                    assertionValue = string.Format("Color.fromBytes({0}, {1}, {2}, {3})", r, g, b, a);
                    valueType = "Color";
                    return;
                }
                case "Rgbaf":
                {
                    int a = GetNumber(0) % 255;
                    int r = GetNumber(1) % 255;
                    int g = GetNumber(2) % 255;
                    int b = GetNumber(3) % 255;

                    value = string.Format("Color.FromArgb({0}, {1}, {2}, {3})", a, r, g, b);
                    assertionValue = string.Format("new Color({0}, {1}, {2}, {3})", r / 255.0, g / 255.0, b / 255.0, a / 255.0);
                    assertionEpsilon = "1e-14";
                    valueType = "Color";
                    return;
                }
                case "VerticalOrigin":
                {
                    value = "CesiumVerticalOrigin.Bottom";
                    assertionValue = "VerticalOrigin.BOTTOM";
                    valueType = "CesiumVerticalOrigin";
                    return;
                }
                case "HorizontalOrigin":
                {
                    value = "CesiumHorizontalOrigin.Left";
                    assertionValue = "HorizontalOrigin.LEFT";
                    valueType = "CesiumHorizontalOrigin";
                    return;
                }
                case "HeightReference":
                {
                    value = "CesiumHeightReference.ClampToGround";
                    assertionValue = "HeightReference.CLAMP_TO_GROUND";
                    valueType = "CesiumHeightReference";
                    return;
                }
                case "LabelStyle":
                {
                    value = "CesiumLabelStyle.FillAndOutline";
                    assertionValue = "LabelStyle.FILL_AND_OUTLINE";
                    valueType = "CesiumLabelStyle";
                    return;
                }
                case "CornerType":
                {
                    value = "CesiumCornerType.Beveled";
                    assertionValue = "CornerType.BEVELED";
                    valueType = "CesiumCornerType";
                    return;
                }
                case "ShadowMode":
                {
                    value = "CesiumShadowMode.CastOnly";
                    assertionValue = "ShadowMode.CAST_ONLY";
                    valueType = "CesiumShadowMode";
                    return;
                }
                case "ColorBlendMode":
                {
                    value = "CesiumColorBlendMode.Replace";
                    assertionValue = "ColorBlendMode.REPLACE";
                    valueType = "CesiumColorBlendMode";
                    return;
                }
                case "StripeOrientation":
                {
                    value = "CesiumStripeOrientation.Vertical";
                    assertionValue = "StripeOrientation.VERTICAL";
                    valueType = "CesiumStripeOrientation";
                    return;
                }
                case "SensorVolumePortionToDisplay":
                {
                    value = "CesiumSensorVolumePortionToDisplay.BelowEllipsoidHorizon";
                    assertionValue = "SensorVolumePortionToDisplay.BELOW_ELLIPSOID_HORIZON";
                    valueType = "CesiumSensorVolumePortionToDisplay";
                    return;
                }
                case "Font":
                {
                    string s = string.Format("{0}px sans-serif", (GetNumber(0) + 5) % 25);
                    value = string.Format("\"{0}\"", s);
                    assertionValue = string.Format("'{0}'", s);
                    valueType = "string";
                    return;
                }
                case "Uri":
                {
                    string s = string.Format("http://example.com/{0}", GetNumber(0));
                    value = string.Format("\"{0}\", CesiumResourceBehavior.LinkTo", s);
                    valueSuffix = ".url";
                    assertionValue = string.Format("'{0}'", s);
                    valueType = "string";
                    return;
                }
                case "NearFarScalar":
                {
                    int nearDistance = GetNumber(0);
                    int nearValue = GetNumber(1);
                    int farDistance = GetNumber(2);
                    int farValue = GetNumber(3);

                    value = string.Format("new NearFarScalar({0}, {1}, {2}, {3})", nearDistance, nearValue, farDistance, farValue);
                    assertionValue = string.Format("new NearFarScalar({0}, {1}, {2}, {3})", nearDistance, nearValue, farDistance, farValue);
                    valueType = "NearFarScalar";
                    return;
                }
                case "DistanceDisplayCondition":
                {
                    int nearDistance = GetNumber(0);
                    int farDistance = GetNumber(1);
                    // near must be less than far
                    if (nearDistance > farDistance)
                    {
                        int temp = nearDistance;
                        nearDistance = farDistance;
                        farDistance = temp;
                    }

                    value = string.Format("new Bounds({0}, {1})", nearDistance, farDistance);
                    assertionValue = string.Format("new DistanceDisplayCondition({0}, {1})", nearDistance, farDistance);
                    valueType = "Bounds";
                    return;
                }
                case "BoundingRectangle":
                {
                    int x = GetNumber(0);
                    int y = GetNumber(1);
                    int width = GetNumber(2);
                    int height = GetNumber(3);

                    value = string.Format("BoundingRectangle.FromWidthHeight({0}, {1}, {2}, {3})", x, y, width, height);
                    assertionValue = string.Format("new BoundingRectangle({0}, {1}, {2}, {3})", x, y, width, height);
                    valueType = "BoundingRectangle";
                    return;
                }
                case "CartographicRectangle":
                {
                    bool isDegrees = valueProperty.Name == "wsenDegrees";
                    double w = GetNumber(0) % (isDegrees ? 45 : Math.PI / 2);
                    double s = GetNumber(1) % (isDegrees ? 45 : Math.PI / 2);
                    double e = GetNumber(2) % (isDegrees ? 45 : Math.PI / 2);
                    double n = GetNumber(3) % (isDegrees ? 45 : Math.PI / 2);

                    value = string.Format("new CartographicExtent({0}, {1}, {2}, {3})", w, s, e, n);
                    assertionValue = string.Format("{4}({0}, {1}, {2}, {3})", w, s, e, n,
                                                   isDegrees ? "Rectangle.fromDegrees" : "new Rectangle");
                    valueType = "CartographicExtent";
                    return;
                }
            }
        }

        private static void NormalizeCartesian(ref double x, ref double y, ref double z)
        {
            double magnitude = Math.Sqrt(x * x + y * y + z * z);

            x /= magnitude;
            y /= magnitude;
            z /= magnitude;
        }

        private static int s_assertionIndent = 0;

        [StringFormatMethod("assertion")]
        private static void WriteAssertion(CodeWriter writer, bool isExtension, string assertion, params object[] args)
        {
            WriteAssertion(writer, isExtension, string.Format(assertion, args));
        }

        private static void WriteAssertion(CodeWriter writer, bool isExtension, string assertion)
        {
            writer.WriteLine("{0}.WriteLine(\"{1}{2}\");", isExtension ? "m_extensionsAssertionsWriter" : "m_assertionsWriter", AssertionIndent, assertion);
        }

        [StringFormatMethod("assertion")]
        private static void WriteAssertionBoth(CodeWriter writer, string assertion, params object[] args)
        {
            WriteAssertionBoth(writer, string.Format(assertion, args));
        }

        private static void WriteAssertionBoth(CodeWriter writer, string assertion)
        {
            writer.WriteLine("WriteAssertionBoth(\"{0}{1}\");", AssertionIndent, assertion);
        }

        private static string AssertionIndent => new string(' ', s_assertionIndent * 4);
    }
}