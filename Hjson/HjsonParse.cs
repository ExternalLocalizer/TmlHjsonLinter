using Newtonsoft.Json.Linq;

namespace ExternalLocalizer.Hjson
{
    internal partial class HjsonCustom
    {
        internal static List<(string, string)> Parse(string context, string prefix)
        {
            var flattened = new List<(string, string)>();
            string jsonString = _parse(context).ToString();

            // Parse JSON
            var jsonObject = JObject.Parse(jsonString);

            foreach (JToken t in jsonObject.SelectTokens("$..*"))
            {
                if (t.HasValues)
                {
                    continue;
                }

                // Due to comments, some objects can by empty
                if (t is JObject obj && obj.Count == 0)
                    continue;

                // Custom implementation of Path to allow "x.y" keys
                string path = "";
                JToken current = t;

                for (JToken? parent = t.Parent; parent != null; parent = parent.Parent)
                {
                    path = parent switch
                    {
                        JProperty property => property.Name + (path == string.Empty ? string.Empty : "." + path),
                        JArray array => array.IndexOf(current) + (path == string.Empty ? string.Empty : "." + path),
                        _ => path
                    };
                    current = parent;
                }

                path = path.Replace(".$parentVal", "");

                flattened.Add((path, t.ToString()));
            }

            return flattened;
        }
    }
}
