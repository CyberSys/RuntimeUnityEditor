﻿using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace RuntimeUnityEditor.Core.Utils.ObjectDumper
{
    /// <summary>
    /// Extensions for dumping objects for debugging purposes.
    /// </summary>
    public static class ObjectDumperExtensions
    {
        /// <summary>
        /// Dumps the object to the console.
        /// </summary>
        public static T DumpToConsole<T>(this T value, string name)
        {
            if (IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            using (var debugWriter = new DebugWriter()) value.Dump(name, debugWriter);

            return value;
        }

        /// <summary>
        /// Dumps the object to a file.
        /// </summary>
        public static T DumpToFile<T>(this T value, string name, string filename)
        {
            return value.DumpToFile(filename, name, Encoding.Default);
        }

        /// <summary>
        /// Dumps the object to a TextWriter.
        /// </summary>
        public static T Dump<T>(this T value, string name, TextWriter writer)
        {
            if (IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            Dumper.Dump(value, name, writer);

            return value;
        }

        /// <summary>
        /// Dumps the object to a file with a specified encoding.
        /// </summary>
        public static T DumpToFile<T>(this T value, string name, string filename, Encoding encoding)
        {
            if (IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (IsNullOrWhiteSpace(filename)) throw new ArgumentNullException(nameof(filename));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            using (var streamWriter = new StreamWriter(filename, false, encoding)) value.Dump(name, streamWriter);

            return value;
        }

        /// <summary>
        /// Dumps the object to a string.
        /// </summary>
        public static string DumpToString<T>(this T value, string name)
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                value.Dump(name, stringWriter);
                return stringWriter.ToString();
            }
        }

        internal static bool IsNullOrWhiteSpace(string value)
        {
            return value == null || value.Trim().Length == 0;
        }

        private sealed class DebugWriter : TextWriter
        {
            public DebugWriter() : base(CultureInfo.InvariantCulture) { }
            public override Encoding Encoding => Console.OutputEncoding;

            public override void Write(char value)
            {
                Console.WriteLine(value);
            }

            public override void Write(string value)
            {
                Console.WriteLine(value);
            }
        }
    }
}