using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace JobReporter2.Helpers
{
    public class XjhParser
    {
        public static DataSet ReadXjh(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("XJH file not found", filePath);

            // Read the file content
            string content = File.ReadAllText(filePath);

            return ParseXjhContent(content);
        }

        public static DataSet ParseXjhContent(string content)
        {
            DataSet dataSet = new DataSet("XjhData");

            // Create tables for each node type found in the file
            Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

            // Regex pattern to match XML-like nodes
            string pattern = @"<(\w+)\s+(.*?)(?:/>|>.*?</\1>)";
            MatchCollection nodeMatches = Regex.Matches(content, pattern, RegexOptions.Singleline);

            foreach (Match nodeMatch in nodeMatches)
            {
                string nodeName = nodeMatch.Groups[1].Value;
                string attributes = nodeMatch.Groups[2].Value;

                // Skip if somehow we found no attributes
                if (string.IsNullOrWhiteSpace(attributes))
                    continue;

                // Create a table for this node type if it doesn't exist
                if (!tables.ContainsKey(nodeName))
                {
                    DataTable table = new DataTable(nodeName);
                    tables[nodeName] = table;
                    dataSet.Tables.Add(table);
                }

                DataTable currentTable = tables[nodeName];

                // Parse attributes
                Dictionary<string, string> attributeDict = ParseAttributes(attributes);

                // Add columns if they don't exist yet
                foreach (var attr in attributeDict)
                {
                    if (!currentTable.Columns.Contains(attr.Key))
                    {
                        currentTable.Columns.Add(attr.Key, typeof(string));
                    }
                }

                // Add a row with the attribute values
                DataRow row = currentTable.NewRow();
                foreach (var attr in attributeDict)
                {
                    row[attr.Key] = attr.Value;
                }
                currentTable.Rows.Add(row);
            }

            return dataSet;
        }

        private static Dictionary<string, string> ParseAttributes(string attributesStr)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();

            // Pattern to match attribute name-value pairs
            // This handles both quoted and unquoted attribute values
            string pattern = @"(\w+)=(?:""([^""]*)""|'([^']*)'|(\S+))";

            MatchCollection matches = Regex.Matches(attributesStr, pattern);

            foreach (Match match in matches)
            {
                string name = match.Groups[1].Value;

                // Value could be in group 2 (double quotes), 3 (single quotes), or 4 (unquoted)
                string value = match.Groups[2].Success ? match.Groups[2].Value :
                               match.Groups[3].Success ? match.Groups[3].Value :
                               match.Groups[4].Value;

                attributes[name] = value;
            }

            return attributes;
        }
    }
}